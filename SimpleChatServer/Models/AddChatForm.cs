namespace SimpleChatServer.Models;

public class AddChatForm
{
    public User Sender { get; set; } = new();
    public Chat RequestedChat { get; set; } = new();
}