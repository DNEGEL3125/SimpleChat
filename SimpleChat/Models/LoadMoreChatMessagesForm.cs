namespace SimpleChat.Models;

public class LoadMoreChatMessagesForm
{
    public ulong ChatId { get; set; }
    public ulong ChatMessageIdSmaller { get; set; }
    public ulong Count { get; set; } = 1;
}