namespace SimpleChatServer.Models;

public class SearchFriendAndGroupChatForm
{
    public string Keyword { get; set; } = string.Empty;
    public bool IsSearchFriend { get; set; }
    public bool IsSearchGroupChat { get; set; }
}