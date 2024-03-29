using System.Data;
using Dapper;
using MySqlConnector;
using SimpleChatServer.Models;

namespace SimpleChatServer.Persistence;

public static class VerificationCodeDao
{
    private const string ConnectionString = "Server=localhost; User ID=root; Password=; Database=simple_chat";


    public static void Save(VerificationCode verificationCode)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "INSERT INTO verification_codes(email, code, send_time) VALUES (@Email, @Code, @SendTime)";
        connection.Execute(sql,
            new { verificationCode.Email, verificationCode.Code, verificationCode.SendTime });
    }

    public static void DeleteVerificationCodeBefore(DateTime dateTime)
    {
        using IDbConnection db = new MySqlConnection(ConnectionString);
        const string sql = "DELETE FROM verification_codes WHERE send_time < @DateTime";

        // Execute the SQL command with Dapper
        db.Execute(sql, new { DateTime = dateTime });
    }

    public static VerificationCode? GetVerificationCodeByEmail(string email)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "SELECT email, code, send_time FROM verification_codes WHERE email = @Email";
        return connection.QueryFirstOrDefault<VerificationCode>(sql, new { Email = email });
    }

    public static int DeleteVerificationCodeByEmail(string email)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "DELETE FROM verification_codes WHERE email = @Email";
        return connection.Execute(sql, new { Email = email });
    }
}