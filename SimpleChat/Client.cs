using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using SimpleChat.Models;

namespace SimpleChat;

public class Client : ReactiveObject, IDisposable
{
    private const int Port = 12345;
    private const string ServerAddress = "127.0.0.1";
    private TcpClient _client;
    private TcpMessage _receivedMsg;
    private GZipStream? _gZipStreamW;
    private GZipStream? _gZipStreamR;
    private StreamWriter? _streamWriter;
    private StreamReader? _streamReader;
    public ReactiveCommand<Unit, TcpMessage> RegisterResCommand { get; }
    public ReactiveCommand<Unit, TcpMessage> LoginResCommand { get; }
    public ReactiveCommand<ChatMessage?, ChatMessage?> ReceiveCommand { get; }
    public ReactiveCommand<Unit, TcpMessage> VerificationCodeResCommand { get; }
    public ReactiveCommand<Unit, TcpMessage> PasswordResetResCommand { get; }
    public ReactiveCommand<Unit, TcpMessage> GetChatListResCommand { get; }

    public ReactiveCommand<List<ChatMessage>?, List<ChatMessage>?>
        LoadMoreChatMessageResCommand { get; }

    public ReactiveCommand<Unit, TcpMessage> SearchFriendResCommand { get; }
    public ReactiveCommand<Unit, TcpMessage> CreateGroupChatResCommand { get; }
    public ReactiveCommand<string?, string?> AddChatResCommand { get; }
    private ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public Client()
    {
        ConnectCommand = ReactiveCommand.CreateFromTask(Connect);
        ReceiveCommand = ReactiveCommand
            .Create<ChatMessage?, ChatMessage?>(chatMessage => chatMessage);
        LoginResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        RegisterResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        VerificationCodeResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        PasswordResetResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        GetChatListResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        LoadMoreChatMessageResCommand =
            ReactiveCommand.Create<List<ChatMessage>?, List<ChatMessage>?>(
                chatMessageList =>
                    chatMessageList
            );
        SearchFriendResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        CreateGroupChatResCommand = ReactiveCommand.Create(() => ReceivedMsg);
        AddChatResCommand = ReactiveCommand.Create<string?, string?>(res => res);
        _client = new TcpClient();
        _receivedMsg = new TcpMessage();

        // Tests connection once every 5 seconds
        Observable.Interval(TimeSpan.FromSeconds(5))
            .Select(_ => Unit.Default)
            .InvokeCommand(ConnectCommand);

        Task.Run(ReceiveMessage);
    }

    private async Task Connect()
    {
        if (_client.Connected)
        {
            return;
        }

        try
        {
            Dispose();
            _client = new TcpClient();
            await _client.ConnectAsync(ServerAddress, Port);
            if (_client.Connected)
            {
                var stream = _client.GetStream();

                _gZipStreamW = new GZipStream(stream, CompressionMode.Compress);
                _gZipStreamR = new GZipStream(stream, CompressionMode.Decompress);
                _streamWriter = new StreamWriter(_gZipStreamW);
                _streamReader = new StreamReader(_gZipStreamR);
                Console.WriteLine("Connect successfully!");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public bool SendMessage(TcpMessage message)
    {
        if (!_client.Connected || _streamWriter == null)
        {
            Console.WriteLine("Can't send message before connect");
            return false;
        }

        try
        {
            var jsonMsg = JsonConvert.SerializeObject(message);
            Console.WriteLine("Send {0}", jsonMsg);

            // Write the message to the stream
            _streamWriter.WriteLine(jsonMsg);
            _streamWriter.Flush(); // Flush to ensure data is sent immediately
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }

        return true;
    }

    private Task ReceiveMessage()
    {
        while (true)
        {
            if (_client is not { Connected: true } || _streamReader == null)
            {
                continue;
            }

            try
            {
                var receivedJson = _streamReader.ReadLine();
                if (string.IsNullOrEmpty(receivedJson))
                {
                    continue;
                }

                Console.WriteLine("收到的数据: {0}", receivedJson);

                var jObjectMsg = JObject.Parse(receivedJson);

                var nullableHeader = jObjectMsg["Header"]?.ToObject<Header>();

                var jsonContent = jObjectMsg["Content"];
                if (jsonContent == null || nullableHeader == null)
                {
                    continue;
                }

                var header = (Header)nullableHeader;

                switch (header)
                {
                    case Header.ChatMessage:
                        // 必须同时有Execute和Subscribe，而且必须写在一起
                        ReceiveCommand.Execute(jsonContent.ToObject<ChatMessage>()).Subscribe();
                        break;
                    case Header.SendChatImage:
                        var chatMessage = jsonContent.ToObject<ChatMessage<ChatImage>>();
                        // 必须同时有Execute和Subscribe，而且必须写在一起
                        ReceiveCommand.Execute(chatMessage?.ToNoTemplateOne()).Subscribe();
                        break;
                    case Header.LogIn:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header, Content = jsonContent.ToObject<User>()
                        };
                        LoginResCommand.Execute().Subscribe();
                        break;
                    case Header.Register:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header, Content = jsonContent.ToObject<RegisterInfo>()
                        };
                        RegisterResCommand.Execute().Subscribe();
                        break;
                    case Header.PasswordResetVerificationCode:
                    case Header.RegisterVerificationCode:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header, Content = jsonContent.ToObject<string>()
                        };
                        VerificationCodeResCommand.Execute().Subscribe();
                        break;
                    case Header.PasswordReset:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header, Content = jsonContent.ToObject<string>()
                        };
                        PasswordResetResCommand.Execute().Subscribe();
                        break;
                    case Header.GetChatList:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header, Content = jsonContent.ToObject<IEnumerable<Chat>>()
                        };
                        GetChatListResCommand.Execute().Subscribe();
                        break;
                    case Header.LoadMoreChatMessage:
                        LoadMoreChatMessageResCommand
                            .Execute(jsonContent.ToObject<List<ChatMessage>>())
                            .Subscribe();
                        break;
                    case Header.SearchFriend:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header,
                            Content = jsonContent.ToObject<SearchFriendAndGroupChatRes>()
                        };
                        SearchFriendResCommand.Execute().Subscribe();
                        break;
                    case Header.CreateGroupChat:
                        ReceivedMsg = new TcpMessage
                        {
                            Header = header, Content = jsonContent.ToObject<Chat>()
                        };
                        CreateGroupChatResCommand.Execute().Subscribe();
                        break;
                    case Header.AddGroupChat:
                        AddChatResCommand
                            .Execute(jsonContent.ToObject<string>())
                            .Subscribe();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }

    public void Dispose()
    {
        // Dispose of existing resources
        _streamWriter?.Dispose();
        _streamReader?.Dispose();
        _gZipStreamW?.Dispose();
        _gZipStreamR?.Dispose();
        _client.Close();
        GC.SuppressFinalize(this);
    }

    private TcpMessage ReceivedMsg
    {
        get => _receivedMsg;
        set => this.RaiseAndSetIfChanged(ref _receivedMsg, value);
    }

    public bool Connected => _client.Connected;
}