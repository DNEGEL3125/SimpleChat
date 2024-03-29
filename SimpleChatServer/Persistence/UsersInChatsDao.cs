using Dapper;
using MySqlConnector;
using SimpleChatServer.Models;

namespace SimpleChatServer.Persistence;

public abstract class UsersInChatsDao : DaoBase
{
    public static IEnumerable<Chat> GetChatsByUserId(ulong userId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "SELECT c.id, c.title FROM users_in_chats INNER JOIN" +
                           " chats c on users_in_chats.chat_id = c.id" +
                           " WHERE user_id = @UserId";
        return connection.Query<Chat>(sql, new { UserId = userId });
    }

    public static IEnumerable<SavedUser> GetUsersByChatId(ulong chatId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "SELECT u.id, u.username, u.hashed_password as HashedPassword," +
                           " u.password_salt as PasswordSalt, u.email FROM users_in_chats " +
                           "INNER JOIN users u ON users_in_chats.user_id = u.id" +
                           " WHERE chat_id = @ChatId";
        return connection.Query<SavedUser>(sql, new { ChatId = chatId });
    }

    public static void Save(Chat chat, User user)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = """
        INSERT INTO users_in_chats(chat_id, user_id) 
        VALUES (@ChatId, @UserId)
        """;
        connection.Execute(sql, new
        {
            UserId = user.Id, ChatId = chat.Id
        });
    }

    public static void Save(Chat chat, IEnumerable<User> usersInChat)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = "INSERT INTO users_in_chats(chat_id, user_id) WITH UserCTE AS(" +
                           "SELECT id as user_id FROM users WHERE email = @Email" +
                           ")" +
                           "SELECT @ChatId, user_id FROM UserCTE";
        // Map each UserId to a new parameter set
        var parameters = usersInChat
            .Select(member => new { ChatId = chat.Id, member.Email });
        connection.Execute(sql, parameters);
    }

    public static Chat? GetChatByUserIdAndChatId(ulong userId, ulong chatId)
    {
        using var db = new MySqlConnection(ConnectionString);
        const string sql =
            "SELECT * FROM users_in_chats " +
            "INNER JOIN chats c on users_in_chats.chat_id = c.id " +
            "WHERE user_id = @UserId AND chat_id = @ChatId";
        return db.QuerySingleOrDefault(sql, new { UserId = userId, ChatId = chatId });
    }
}