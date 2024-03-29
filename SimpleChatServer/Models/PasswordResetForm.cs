using SimpleChatServer.Utils;

namespace SimpleChatServer.Models;

public class PasswordResetForm
{
    public string Email { get; set; } = string.Empty;
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
            PasswordSalt = strSalt
        };
    }
}