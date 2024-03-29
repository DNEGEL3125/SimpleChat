namespace SimpleChatServer.Models;

public class SearchFriendAndGroupChatRes
{
    public IEnumerable<Chat> MatchingChats { get; set; } = new List<Chat>();
    public IEnumerable<User> MatchingUsers { get; set; } = new List<User>();
}