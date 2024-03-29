using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using DynamicData;
using ReactiveUI;
using SimpleChat.Models;
using SimpleChat.Utils;
using SimpleChat.Views;

namespace SimpleChat.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    // Storage the account
    private User? Account { get; set; }

    // private readonly ChatViewModel _chatViewModel;
    private readonly ChatListViewModel _chatListViewModel;

    private Client MyClient { get; } = new();

    // Current View Model
    private ViewModelBase _contentViewModel = null!;

    private static MainWindow? GetMainWindow() =>
        Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow as MainWindow
            : null;

    private static IStorageProvider? StorageProvider => GetMainWindow()?.StorageProvider;


    public MainWindowViewModel()
    {
        _chatListViewModel = new ChatListViewModel(ShowCreateGroupChatWindow, ShowAddFriendWindow);
        _chatListViewModel.ShowChatViewCommand
            .Subscribe(ShowChatView);
        // ContentViewModel will initialize in this function
        Login();
    }

    private void Login()
    {
        LoginViewModel loginViewModel = new();

        loginViewModel.LoginCommand.Subscribe(user =>
        {
            SendTcpMsg(user, Header.LogIn);

            MyClient.LoginResCommand
                .Take(1)
                .Subscribe(msg =>
                    {
                        if (msg.Content == null)
                        {
                            AvaloniaChatExtension.ShowMessageBox(
                                "Incorrect username or password.",
                                "Fail"
                            );
                        }
                        else
                        {
                            Account = msg.Content as User;
                            ShowChatListView();
                        }
                    }
                );
        });

        loginViewModel.RegisterCommand.Take(1).Subscribe(_ => Register());
        loginViewModel.PasswordResetCommand.Subscribe(_ => PasswordReset());
        ContentViewModel = loginViewModel;
    }

    private void PasswordReset()
    {
        PasswordResetViewModel passwordResetViewModel = new();

        passwordResetViewModel.LoginCommand.Subscribe(_ => Login());

        passwordResetViewModel.PasswordResetCommand.Subscribe(passwordResetForm =>
        {
            SendTcpMsg(passwordResetForm, Header.PasswordReset);
            MyClient.PasswordResetResCommand
                .Take(1)
                .Subscribe(msg =>
                {
                    if (msg.Content is not string result)
                    {
                        return;
                    }

                    if (result == string.Empty)
                    {
                        AvaloniaChatExtension.ShowMessageBox("Go to log in.", "Success");
                        Login();
                    }
                    else
                    {
                        AvaloniaChatExtension.ShowMessageBox(result, "Fail");
                    }
                });
        });

        passwordResetViewModel.GetVerificationCodeCommand
            .Subscribe(email =>
            {
                MyClient.VerificationCodeResCommand
                    .Take(1)
                    .Subscribe(msg =>
                    {
                        var result = msg.Content as string;
                        switch (result)
                        {
                            case null:
                                return;
                            case "":
                                AvaloniaChatExtension.ShowMessageBox(
                                    text: "The verification code has sent to your email",
                                    title: "Success"
                                );
                                break;
                            default:
                                AvaloniaChatExtension.ShowMessageBox(text: result, title: "Fail");
                                break;
                        }
                    });

                var success = SendTcpMsg(email, Header.PasswordResetVerificationCode);
                if (success)
                    passwordResetViewModel.ResetVerificationCodeCooldown();
            });

        ContentViewModel = passwordResetViewModel;
    }

    private void Register()
    {
        RegisterViewModel registerViewModel = new();

        registerViewModel.GetVerificationCodeCommand
            .Subscribe(email =>
            {
                registerViewModel.ResetVerificationCodeCooldown();

                MyClient.VerificationCodeResCommand
                    .Take(1)
                    .Subscribe(msg =>
                    {
                        var result = msg.Content as string;
                        switch (result)
                        {
                            case null:
                                return;
                            case "":
                                AvaloniaChatExtension.ShowMessageBox(
                                    "The verification code has sent to your email",
                                    "Success"
                                );
                                break;
                            default:
                                AvaloniaChatExtension.ShowMessageBox(result,
                                    "Fail");
                                registerViewModel.ClearVerificationCodeCooldown();
                                break;
                        }
                    });

                SendTcpMsg(email, Header.RegisterVerificationCode);
            });

        registerViewModel.RegisterCommand
            .Subscribe(registerForm =>
            {
                SendTcpMsg(registerForm, Header.Register);

                MyClient.RegisterResCommand
                    .Take(1)
                    .Subscribe(onNext: msg =>
                    {
                        if (msg.Content is not RegisterInfo result)
                        {
                            return;
                        }

                        // Success
                        if (result.Msg == string.Empty)
                        {
                            Account = result.Account;
                            ShowChatListView();
                        }
                        else
                        {
                            AvaloniaChatExtension.ShowMessageBox(result.Msg, "Fail");
                        }
                    });
            });

        registerViewModel.LoginCommand.Subscribe(_ => Login());

        ContentViewModel = registerViewModel;
    }


    private void ShowChatView(Chat chat)
    {
        Console.WriteLine(chat);
        if (!MyClient.Connected)
        {
            return;
        }

        var chatViewModel = new ChatViewModel(chat, StorageProvider);


        chatViewModel.LoadMoreMessagesCommand
            .Subscribe(form =>
            {
                MyClient.LoadMoreChatMessageResCommand
                    .Take(1)
                    .Subscribe(chatMessageList =>
                    {
                        if (chatMessageList == null)
                        {
                            return;
                        }

                        if (Account == null)
                        {
                            Login();
                            return;
                        }

                        if (chatMessageList.Count == 0)
                        {
                            // Has loaded all messages
                            chatViewModel.ShouldLoadMoreMessages = false;
                        }

                        foreach (var chatMessage in chatMessageList)
                        {
                            if (chatMessage.Sender.Email == Account.Email)
                                chatMessage.Sender.IsSelf = true;

                            if (chatMessage.Type == ChatMessageType.Image)
                            {
                                if (chatMessage.Content is not string filePath)
                                    continue;

                                chatMessage.Content =
                                    $"""![{chatMessage.Sender.Username}'s image]({filePath})""";

                                Console.WriteLine(chatMessage.Content);
                            }
                        }

                        chatViewModel.OnLoadingMoreMessageEvent();

                        // Add the loaded messages to ChatView
                        chatViewModel.ListMessages.AddOrInsertRange(chatMessageList, 0);


                        // Load enough messages
                        if (chatViewModel.ShouldLoadMoreMessages)
                        {
                            chatViewModel.LoadMoreMessagesCommand
                                .Execute()
                                .Subscribe();
                        }
                    });
                SendTcpMsg(form, Header.LoadMoreChatMessage);
            });


        chatViewModel.SendCommand
            .Subscribe(msg =>
            {
                if (Account == null)
                {
                    // Can't send a msg before login
                    Login();
                    return;
                }

                MyClient.ReceiveCommand
                    .Take(1)
                    .Subscribe(
                        chatMessage =>
                        {
                            if (chatMessage == null)
                            {
                                return;
                            }

                            if (Account == null)
                            {
                                Login();
                                return;
                            }

                            switch (chatMessage.Content)
                            {
                                case string:
                                    break;
                                case ChatImage imgContent:
                                {
                                    AvaloniaChatExtension.SaveChatImage(imgContent);
                                    // To markdown
                                    chatMessage.Content = imgContent.ToMarkDown(chatMessage.Sender.Username);
                                    break;
                                }
                                default:
                                    return;
                            }

                            if (chatMessage.Sender.Email == Account.Email)
                            {
                                chatMessage.Sender.IsSelf = true;
                                // Clear the user input if the message is sent successfully
                                chatViewModel.MessageInput = string.Empty;
                            }

                            chatViewModel.OnReceiveChatMessageEvent();
                            chatViewModel.ListMessages.Add(chatMessage);
                        });


                msg.Sender = Account;
                SendTcpMsg(msg, Header.ChatMessage);
            });

        chatViewModel.SendImgCommand
            .Subscribe(msg =>
            {
                if (Account == null)
                {
                    // Can't send a msg before login
                    Login();
                    return;
                }

                msg.Sender = Account;

                SendTcpMsg(msg, Header.SendChatImage);
            });

        chatViewModel.BackCommand
            .Subscribe(_ => ShowChatListView());

        ContentViewModel = chatViewModel;
    }

    private void ShowChatListView()
    {
        MyClient.GetChatListResCommand
            .Take(1)
            .Subscribe(msg =>
            {
                if (msg.Content is not IEnumerable<Chat> chatList)
                {
                    return;
                }

                // Update the chat list
                _chatListViewModel.ChatList.Clear();
                _chatListViewModel.ChatList.Add(chatList);
            });
        SendTcpMsg(Account, Header.GetChatList);

        ContentViewModel = _chatListViewModel;
    }

    private void ShowCreateGroupChatWindow()
    {
        var mainWindow = GetMainWindow();
        var createGroupChatWindowViewModel = new CreateGroupChatWindowViewModel();
        var createGroupChatWindowView = new CreateGroupChatWindowView
        {
            DataContext = createGroupChatWindowViewModel,
            Width = 300
        };

        createGroupChatWindowViewModel.CreateGroupChatCommand
            .Subscribe(form =>
            {
                if (Account == null)
                {
                    Login();
                    return;
                }

                MyClient.CreateGroupChatResCommand
                    .Take(1)
                    .Subscribe(msg =>
                    {
                        if (msg.Content is not Chat createdChat)
                        {
                            AvaloniaChatExtension.ShowMessageBox("Error create group chat");
                            return;
                        }

                        AvaloniaChatExtension.ShowMessageBox("Create successfully!", title: "Success");
                        // Renew the chat list
                        _chatListViewModel.ChatList.Add(createdChat);
                        createGroupChatWindowView.Close();
                    });

                form.Members = new List<User> { Account };

                SendTcpMsg(form, Header.CreateGroupChat);
            });

        if (mainWindow == null)
        {
            createGroupChatWindowView.Show();
        }
        else
        {
            createGroupChatWindowView.Show(mainWindow);
        }
    }

    private void ShowAddFriendWindow()
    {
        var mainWindow = GetMainWindow();

        var addFriendWindowViewModel = new AddFriendWindowViewModel();

        var addFriendWindowView = new AddFriendWindowView
        {
            DataContext = addFriendWindowViewModel,
            Width = 500
        };

        MyClient.AddChatResCommand
            .Subscribe(res =>
            {
                if (string.IsNullOrEmpty(res))
                {
                    AvaloniaChatExtension.ShowMessageBox("Success", "Result");
                    ShowChatListView();
                    return;
                }

                AvaloniaChatExtension.ShowMessageBox(res, "Fail");
            });

        addFriendWindowViewModel.AddChatCommand
            .Subscribe(chat =>
            {
                if (Account == null)
                {
                    Login();
                    return;
                }

                var form = new AddChatForm
                {
                    RequestedChat = chat,
                    Sender = Account
                };
                SendTcpMsg(form, Header.AddGroupChat);
            });

        addFriendWindowViewModel.SearchFriendCommand
            .Subscribe(form => SendTcpMsg(form, Header.SearchFriend));

        MyClient.SearchFriendResCommand
            .Subscribe(msg =>
            {
                if (msg.Content is not SearchFriendAndGroupChatRes res)
                {
                    return;
                }

                addFriendWindowViewModel.MatchingUsers.Clear();
                addFriendWindowViewModel.MatchingChats.Clear();

                addFriendWindowViewModel.MatchingUsers.Add(res.MatchingUsers);
                addFriendWindowViewModel.MatchingChats.Add(res.MatchingChats);
            });

        if (mainWindow != null)
            addFriendWindowView.Show(mainWindow);
        else
            addFriendWindowView.Show();
    }

    private bool SendTcpMsg(object? content, Header header)
    {
        return MyClient.SendMessage(
            new TcpMessage
            {
                Content = content,
                Header = header,
                Sender = Account
            }
        );
    }


    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }


#pragma warning restore CA1822 // Mark members as static
}