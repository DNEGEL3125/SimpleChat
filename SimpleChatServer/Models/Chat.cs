using Newtonsoft.Json;

namespace SimpleChatServer.Models;

public class Chat
{
    public ulong Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}