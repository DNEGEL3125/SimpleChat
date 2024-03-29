using LiteDB;
using Newtonsoft.Json;

namespace SimpleChat.Models;

public class Chat
{
    [BsonId] public ulong Id { get; set; }
    public string Title { get; set; } = string.Empty;


    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}