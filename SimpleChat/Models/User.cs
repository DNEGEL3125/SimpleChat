using Newtonsoft.Json;

namespace SimpleChat.Models;

public class User
{
    public ulong Id { get; set; }

    // 是否是当前用户
    public bool IsSelf { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    // public Image Avatar { get; set; } = new Image();

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}