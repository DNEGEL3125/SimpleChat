namespace SimpleChat.Models;

public enum Header
{
    ChatMessage,
    LogIn,
    Register,
    RegisterVerificationCode,
    PasswordReset,
    PasswordResetVerificationCode,
    GetChatList,
    LoadMoreChatMessage,
    SearchFriend,
    CreateGroupChat,
    AddGroupChat,
    SendChatImage
}

public class TcpMessage
{
    public object? Content { get; set; }
    public Header Header { get; set; }
    public User? Sender { get; set; }
}