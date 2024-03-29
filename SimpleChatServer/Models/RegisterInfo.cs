namespace SimpleChatServer.Models;

public class RegisterInfo
{
    public string Msg { get; set; } = string.Empty;
    public User? Account { get; set; }

    public override string ToString()
    {
        return Msg;
    }
}