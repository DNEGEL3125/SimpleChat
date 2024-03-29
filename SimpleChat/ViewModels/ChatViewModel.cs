using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Timers;
using Avalonia.Platform.Storage;
using ReactiveUI;
using SimpleChat.Models;
using SimpleChat.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace SimpleChat.ViewModels;

public class ChatViewModel : ViewModelBase
{
    public static int MaxMessageLen => 1000;
    private string _messageInput = string.Empty;
    private double _bubbleTipOpacity;
    private string _bubbleTipText = string.Empty;
    private int _messageInputIndex;

    private Chat CurChat { get; }

    // Load message directly after a message loaded
    // Be true until scroll can drag
    public bool ShouldLoadMoreMessages { get; set; } = true;
    private IStorageProvider StorageProvider { get; }
    public ReactiveCommand<Unit, Unit> BackCommand { get; set; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, ChatMessage> SendCommand { get; }
    public ReactiveCommand<ChatMessage, ChatMessage> SendImgCommand { get; }
    public ReactiveCommand<Unit, Unit> InsertNewLineCommand { get; }
    public ReactiveCommand<Unit, LoadMoreChatMessagesForm> LoadMoreMessagesCommand { get; }
    public ReactiveCommand<Unit, Unit> MoreFileFormatCommand { get; }
    public ObservableCollection<ChatMessage> ListMessages { get; set; } = new();
    public event EventHandler ReceiveChatMessageEvent = null!;
    public event EventHandler? LoadingMoreMessageEvent;

    private readonly Timer _bubbleTipOpacityTimer = new(TimeSpan.FromSeconds(0.1));

    public ChatViewModel(Chat chat, IStorageProvider? storageProvider)
    {
        CurChat = chat;
        StorageProvider = storageProvider ?? throw new ArgumentNullException();
        var isValidObservable = this.WhenAnyValue<ChatViewModel, bool, string>(
            x => x.MessageInput,
            x => !string.IsNullOrEmpty(x));
        SendCommand = ReactiveCommand.Create(
            () => new ChatMessage
            {
                Content = MessageInput,
                ChatId = chat.Id,
                Type = ChatMessageType.Text
            },
            canExecute: isValidObservable
        );
        InsertNewLineCommand = ReactiveCommand.Create(() =>
            {
                MessageInput = MessageInput
                    .Insert(MessageInputIndex, Environment.NewLine);
                MessageInputIndex++;
            }
        );
        LoadMoreMessagesCommand = ReactiveCommand.Create(() =>
            ListMessages.Count == 0
                ? new LoadMoreChatMessagesForm
                    { ChatId = chat.Id, ChatMessageIdSmaller = ulong.MaxValue }
                : new LoadMoreChatMessagesForm
                    { ChatId = chat.Id, ChatMessageIdSmaller = ListMessages.First().Id });

        SendImgCommand = ReactiveCommand.Create<ChatMessage, ChatMessage>(msg => msg);

        MoreFileFormatCommand = ReactiveCommand.Create(ShowOpenFileDialog);
        // Make bubble tip disappear slowly
        _bubbleTipOpacityTimer.Elapsed += (_, _) =>
        {
            if (BubbleTipOpacity >= 0.1)
            {
                BubbleTipOpacity -= 0.05;
            }
            else
            {
                BubbleTipOpacity = 0;
            }
        };
        _bubbleTipOpacityTimer.Start();
    }

    public ChatViewModel()
    {
        SendCommand = null!;
        LoadMoreMessagesCommand = null!;
        MoreFileFormatCommand = null!;
        InsertNewLineCommand = null!;
        SendImgCommand = null!;
        StorageProvider = null!;
        CurChat = new Chat { Title = "ChatView Preview" };
        ListMessages = new ObservableCollection<ChatMessage>
        {
            new() { Content = "abc", Sender = new User { Username = "Alpha" } },
            new() { Content = "3.1415926", Sender = new User { Username = "Math teacher" } },
            new()
            {
                Content =
                    "Very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very long",
                Sender = new User { Username = "V" }
            },
            new()
            {
                Content =
                    "![Log in page](../../../Resources/Images/Demo/login.png)",
                Sender = new User { Username = "Markdown image test" }
            }
        };
    }


    private async void ShowOpenFileDialog()
    {
        const int maxEdgeLength = 512; // Maximum edge length

        var filePickerOpenOptions = new FilePickerOpenOptions
        {
            Title = "Select a file",
            FileTypeFilter = new List<FilePickerFileType>
            {
                FilePickerFileTypes.ImageAll
            }
        };
        var result = await StorageProvider
            .OpenFilePickerAsync(filePickerOpenOptions);

        var path = result[0].Path.LocalPath;
        var fileExtension = Path.GetExtension(path);

        var image = await Image.LoadAsync(path);
        if (image.Width > maxEdgeLength || image.Height > maxEdgeLength)
        {
            var ratio = maxEdgeLength / float.Max(image.Width, image.Height);
            image.Mutate(x => x.Resize((Size)(image.Size * ratio)));
        }

        // Convert image to byte array
        byte[] imageData;
        using (var ms = new MemoryStream())
        {
            IImageEncoder encoder;
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                    encoder = new JpegEncoder();
                    break;
                case ".png":
                    encoder = new PngEncoder();
                    break;
                case ".gif":
                    encoder = new GifEncoder();
                    break;
                default:
                    AvaloniaChatExtension.ShowMessageBox("Unsupported image format");
                    return;
            }

            await image.SaveAsync(ms, encoder); // Replace "image" with your Image object
            imageData = ms.ToArray();
        }

        // Console.WriteLine(imageData);

        var msg = new ChatMessage
        {
            Content = new ChatImage
            {
                Base64Data = Convert.ToBase64String(imageData),
                FilePath = path
            },
            ChatId = CurChat.Id,
            Type = ChatMessageType.Image
        };

        SendImgCommand.Execute(msg)
            .Subscribe();
    }

    public string Title => CurChat.Title;

    public string MessageInput
    {
        get => _messageInput;
        set => this.RaiseAndSetIfChanged(ref _messageInput, value);
    }

    public double BubbleTipOpacity
    {
        get => _bubbleTipOpacity;
        set => this.RaiseAndSetIfChanged(ref _bubbleTipOpacity, value);
    }

    public string BubbleTipText
    {
        get => _bubbleTipText;
        set => this.RaiseAndSetIfChanged(ref _bubbleTipText, value);
    }

    public int MessageInputIndex
    {
        get => _messageInputIndex;
        set => this.RaiseAndSetIfChanged(ref _messageInputIndex, value);
    }

    public void OnReceiveChatMessageEvent()
    {
        ReceiveChatMessageEvent(this, EventArgs.Empty);
    }

    // public ObservableCollection<HorizontalAlignment> ChatMessageAlignments { get; set; }
    public void OnLoadingMoreMessageEvent()
    {
        LoadingMoreMessageEvent?.Invoke(this, EventArgs.Empty);
    }
}