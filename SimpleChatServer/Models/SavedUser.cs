namespace SimpleChatServer.Models;

public class SavedUser
{
    public ulong Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public User ToUser()
    {
        return new User
        {
            Id = Id,
            Email = Email,
            Username = Username
        };
    }
}