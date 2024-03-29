using System.Security.Cryptography;

namespace SimpleChatServer.Utils;

public class Encryptor
{
    public static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        RandomNumberGenerator.Create().GetBytes(salt);
        return salt;
    }

    public static string HashPassword(string password, byte[]? salt = null)
    {
        salt ??= GenerateSalt();

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32); // 32 bytes = 256 bits
        var combinedBytes = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combinedBytes, salt.Length, hash.Length);

        return Convert.ToBase64String(combinedBytes);
    }
}