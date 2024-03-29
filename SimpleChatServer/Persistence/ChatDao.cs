using Dapper;
using MySqlConnector;
using SimpleChatServer.Models;

namespace SimpleChatServer.Persistence;

public abstract class ChatDao : DaoBase
{
    /// <returns>The created chat</returns>
    public static Chat CreateChatByTitle(string title)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "INSERT INTO chats(title) VALUES (@Title); " +
                           "SELECT LAST_INSERT_ID();";
        var chatId = connection.ExecuteScalar<ulong>(sql, new { Title = title });
        return new Chat { Id = chatId, Title = title };
    }

    public static IEnumerable<Chat> SearchChatsByTitle(string titleKeyword)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "SELECT id, title FROM chats WHERE title LIKE @TitleKeyword";
        return connection.Query<Chat>(sql, new { TitleKeyword = $"%{titleKeyword}%" });
    }
}