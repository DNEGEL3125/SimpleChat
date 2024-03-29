using SimpleChatServer.Utils;

namespace SimpleChatServer.Models;

public class RegisterForm
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;

    public SavedUser ToSavedUser()
    {
        var salt = Encryptor.GenerateSalt();
        var hashedPassword = Encryptor.HashPassword(Password, salt);
        var strSalt = Convert.ToBase64String(salt);
        return new SavedUser
        {
            Email = Email,
            HashedPassword = hashedPassword,
            PasswordSalt = strSalt,
            Username = Username
        };
    }

    public User ToUser()
    {
        return new User
        {
            Email = Email,
            Password = Password,
            Username = Username
        };
    }
}