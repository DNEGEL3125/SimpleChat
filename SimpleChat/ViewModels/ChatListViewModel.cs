using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using SimpleChat.Models;

namespace SimpleChat.ViewModels;

public class ChatListViewModel : ViewModelBase
{
    public ObservableCollection<Chat> ChatList { get; } = new();
    public ReactiveCommand<Chat, Chat> ShowChatViewCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ShowAddFriendWindowCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowCreateGroupChatWindowCommand { get; }

    public ChatListViewModel(Action showCreateGroupChatWindowAction, Action showAddFriendWindowAction)
    {
        ShowChatViewCommand = ReactiveCommand.Create<Chat, Chat>(chat => chat);
        ShowAddFriendWindowCommand = ReactiveCommand.Create(showAddFriendWindowAction);
        ShowCreateGroupChatWindowCommand = ReactiveCommand.Create(showCreateGroupChatWindowAction);
    }

    /// <summary>
    /// Only for display
    /// </summary>
    public ChatListViewModel()
    {
        ShowChatViewCommand = null!;
        ShowAddFriendWindowCommand = null!;
        ShowCreateGroupChatWindowCommand = null!;
        ChatList = new ObservableCollection<Chat>()
        {
            new() { Title = "Good chat" },
            new() { Title = "Great chat" },
            new() { Title = "Prince One Cafe" },
            new() { Title = "ZNCTF-2024" }
        };
    }
}