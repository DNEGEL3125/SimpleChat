using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleChatServer.Models;
using SimpleChatServer.Services;

namespace SimpleChatServer;

internal class ChatClient : IDisposable
{
    public ChatClient(TcpClient client)
    {
        Client = client;
        var stream = client.GetStream();
        var gzipStreamR = new GZipStream(
            stream,
            CompressionMode.Decompress,
            leaveOpen: true
        );
        var gzipStreamW = new GZipStream(
            stream,
            CompressionMode.Compress,
            leaveOpen: true
        );
        Reader = new StreamReader(gzipStreamR);
        Writer = new StreamWriter(gzipStreamW);

        RemoteEndPoint = Client.Client.RemoteEndPoint?.ToString() ?? string.Empty;
    }

    public void Dispose()
    {
        Writer.Dispose();
        Reader.Dispose();
        Client.Close();
        GC.SuppressFinalize(this);
    }

    public string RemoteEndPoint { get; }

    public bool Connected => Client.Connected;

    private TcpClient Client { get; }
    public User? User { get; set; }
    public StreamReader Reader { get; }
    public StreamWriter Writer { get; }
}

public class Server
{
    private const int Port = 12345;
    private readonly IPAddress _ipAddress = IPAddress.Any;
    private readonly Dictionary<string, ChatClient> _emailToClient = new();
    private readonly VerificationCodeService _verificationCodeService = new();

    public Server()
    {
        TcpListener? server = null;
        try
        {
            server = new TcpListener(_ipAddress, Port);
            server.Start();
            Console.WriteLine("服务器，启动！");
            Console.WriteLine("Waiting for connection...");

            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine("A connection has been established. Address = {0}",
                    client.Client.RemoteEndPoint);

                // 为每一个client创建线程
                var clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        finally
        {
            server?.Stop();
        }
    }

    private void HandleClient(TcpClient client)
    {
        using var chatClient = new ChatClient(client);
        try
        {
            while (chatClient.Connected)
            {
                var readStr = chatClient.Reader.ReadLine();
                if (readStr == null)
                {
                    break;
                }

                Console.WriteLine("Data received: {0}", readStr);

                var tcpMessage = JsonConvert.DeserializeObject<TcpMessage>(readStr);
                if (tcpMessage == null)
                {
                    continue;
                }

                var sender = tcpMessage.Sender;
                var jsonContent = tcpMessage.Content as JObject;
                var header = tcpMessage.Header;

                if (sender != null && !string.IsNullOrEmpty(sender.Email))
                {
                    _emailToClient[sender.Email] = chatClient;
                    chatClient.User = sender;
                }


                switch (header)
                {
                    case Header.ChatMessage:
                    {
                        var chatMessage = jsonContent?.ToObject<ChatMessage<string>>();
                        ChatMessageService.SaveMessage(chatMessage);
                        SendChatMessage(chatMessage);
                        break;
                    }
                    case Header.LogIn:
                    {
                        var account = jsonContent?.ToObject<User>();
                        var res = UserService.LogIn(account);
                        if (res != null)
                        {
                            // Login is successful
                            _emailToClient[res.Email] = chatClient;
                        }

                        SendObjectToClient(res, Header.LogIn, chatClient);
                        break;
                    }
                    case Header.Register:
                    {
                        var registerForm = jsonContent?.ToObject<RegisterForm>();
                        var registerInfo = UserService.Register(registerForm);
                        SendObjectToClient(
                            registerInfo,
                            Header.Register,
                            chatClient
                        );
                        break;
                    }
                    case Header.PasswordResetVerificationCode:
                    case Header.RegisterVerificationCode:
                    {
                        var email = jsonContent?.ToObject<string>();
                        var res = _verificationCodeService.SendVerificationCode(email, header);
                        SendObjectToClient(
                            res,
                            Header.RegisterVerificationCode,
                            chatClient
                        );
                        break;
                    }
                    case Header.PasswordReset:
                    {
                        var passwordResetForm = jsonContent?.ToObject<PasswordResetForm>();
                        var res = UserService.PasswordReset(passwordResetForm);
                        SendObjectToClient(
                            res,
                            Header.PasswordReset,
                            chatClient
                        );
                        break;
                    }
                    case Header.GetChatList:
                    {
                        var user = jsonContent?.ToObject<User>();
                        var chatList = UsersInChatsService
                            .GetChatsByUser(user);
                        SendObjectToClient(
                            chatList,
                            Header.GetChatList,
                            chatClient
                        );
                        break;
                    }
                    case Header.LoadMoreChatMessage:
                    {
                        var form = jsonContent?.ToObject<LoadMoreChatMessagesForm>();
                        var chatMessageList = ChatMessageService.SolveLoadMoreMessageRequest(form);
                        SendObjectToClient(
                            chatMessageList,
                            Header.LoadMoreChatMessage,
                            chatClient
                        );

                        break;
                    }
                    case Header.SearchFriend:
                    {
                        var form = jsonContent?.ToObject<SearchFriendAndGroupChatForm>();
                        var matchingRes = UserService.SearchFriendAndGroupChat(form);
                        SendObjectToClient(matchingRes, header, chatClient);
                        break;
                    }
                    case Header.CreateGroupChat:
                    {
                        var form = jsonContent?.ToObject<CreateChatForm>();
                        var createdChat = ChatService.CreateChat(form);
                        SendObjectToClient(createdChat, header, chatClient);
                        break;
                    }
                    case Header.AddGroupChat:
                    {
                        var form = jsonContent?.ToObject<AddChatForm>();
                        var res = UsersInChatsService.SolveAddChatForm(form);
                        SendObjectToClient(res, header, chatClient);
                        break;
                    }
                    case Header.SendChatImage:
                    {
                        var chatMessage = jsonContent?.ToObject<ChatMessage<ChatImage>>();
                        ChatImageService.SolveChatImage(chatMessage);
                        SendChatMessage(chatMessage, header);
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        finally
        {
            // 离开后从客户端列表中移除
            var emailOfClient = chatClient.User?.Email;
            if (!string.IsNullOrEmpty(emailOfClient))
            {
                _emailToClient.Remove(emailOfClient);
            }

            Console.WriteLine(
                "A chatClient has left. Client address: {0}, Email: {1}",
                chatClient.RemoteEndPoint,
                emailOfClient
            );
        }
    }

    private static void SendTcpMessageToClient(TcpMessage tcpMessage, ChatClient client)
    {
        var json = JsonConvert.SerializeObject(tcpMessage);
        var writer = client.Writer;
        writer.WriteLine(json);
        writer.Flush();
    }

    private static void SendObjectToClient(object? obj, Header header, ChatClient client)
    {
        var msg = new TcpMessage
        {
            Header = header,
            Content = obj
        };
        SendTcpMessageToClient(
            msg, client
        );
    }

    private void SendChatMessage<T>(ChatMessage<T>? msg, Header header = Header.ChatMessage)
    {
        if (msg == null)
        {
            return;
        }

        // Find users in the chat
        var targetUsers = UsersInChatsService
            .GetUsersByChatId(msg.ChatId);
        var targetEmails = targetUsers
            .Select(x => x.Email);

        foreach (var email in targetEmails)
        {
            // One by one lookup, sent to currently logged in users
            if (!_emailToClient.TryGetValue(email, out var client)) continue;
            SendObjectToClient(msg, header, client);
        }
    }
}