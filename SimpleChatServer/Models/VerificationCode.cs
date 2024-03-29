namespace SimpleChatServer.Models;

public class VerificationCode
{
    public VerificationCode()
    {
        Code = Random.Shared.Next(100000, 999999).ToString();
    }

    public DateTime SendTime { get; set; } = DateTime.Now;
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; }

    public override string ToString()
    {
        return Code;
    }
}