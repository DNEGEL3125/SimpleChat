using Newtonsoft.Json;

namespace SimpleChatServer.Models;

public enum ChatMessageType
{
    Unknown,
    Text,
    Image,
    File
}

public class ChatMessage<T>
{
    public ulong Id { get; set; }
    public ulong ChatId { get; set; }
    public T? Content { get; set; }
    public User Sender { get; set; } = new();
    public DateTime SendDateTime { get; set; } = DateTime.Now;
    public ChatMessageType Type { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}