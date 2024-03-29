using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleChat.Models;

namespace SimpleChat.ViewModels;

public class AddFriendWindowViewModel : ViewModelBase
{
    private string _searchKeyword = string.Empty;
    private bool _shouldSearchFriend;
    private bool _shouldSearchGroupChat;
    public ObservableCollection<Chat> MatchingChats { get; set; } = new();
    public ObservableCollection<User> MatchingUsers { get; set; } = new();
    public ReactiveCommand<User, User> AddFriendCommand { get; }
    public ReactiveCommand<Chat, Chat> AddChatCommand { get; }
    public ReactiveCommand<SearchFriendAndGroupChatForm, SearchFriendAndGroupChatForm> SearchFriendCommand { get; }

    public AddFriendWindowViewModel()
    {
        var isValidObservable = this.WhenAnyValue(
            x => x.SearchKeyword,
            x => x.ShouldSearchFriend,
            x => x.ShouldSearchGroupChat,
            (keyword, isSearchFriend, isSearchGroupChat) =>
                keyword != string.Empty && (isSearchFriend || isSearchGroupChat));
        SearchFriendCommand =
            ReactiveCommand.Create<SearchFriendAndGroupChatForm, SearchFriendAndGroupChatForm>(
                form => form, isValidObservable
            );
        AddFriendCommand = ReactiveCommand.Create<User, User>(user => user);
        AddChatCommand = ReactiveCommand.Create<Chat, Chat>(chat => chat);
        // Subscribe to the PropertyChanged event to monitor changes in SearchKeyword
        this.WhenAnyValue(
                x => x.SearchKeyword,
                x => x.ShouldSearchFriend,
                x => x.ShouldSearchGroupChat,
                (keyword, isSearchFriend, isSearchGroupChat) => new SearchFriendAndGroupChatForm
                {
                    IsSearchFriend = isSearchFriend,
                    IsSearchGroupChat = isSearchGroupChat,
                    Keyword = keyword
                }
            )
            .Throttle(TimeSpan.FromSeconds(0.5)) // Throttle to avoid rapid consecutive searches
            .InvokeCommand(SearchFriendCommand);
    }

    public string SearchKeyword
    {
        get => _searchKeyword;
        set => this.RaiseAndSetIfChanged(ref _searchKeyword, value);
    }

    public bool ShouldSearchFriend
    {
        get => _shouldSearchFriend;
        set => this.RaiseAndSetIfChanged(ref _shouldSearchFriend, value);
    }

    public bool ShouldSearchGroupChat
    {
        get => _shouldSearchGroupChat;
        set => this.RaiseAndSetIfChanged(ref _shouldSearchGroupChat, value);
    }
}