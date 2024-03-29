using Dapper;
using MySqlConnector;
using SimpleChatServer.Models;

namespace SimpleChatServer.Persistence;

public abstract class UserDao : DaoBase
{
    public static SavedUser? GetUserByEmail(string email)
    {
        using var connection = new MySqlConnection(ConnectionString);
        // 定义 SQL 查询语句
        const string sql = """
        SELECT id, username, hashed_password as HashedPassword,
        password_salt as PasswordSalt, email FROM users WHERE email = @Email
""";

        // 执行 SQL 查询并获取第一个用户
        var user = connection.QueryFirstOrDefault<SavedUser>(sql, new { Email = email });

        return user;
    }

    public static SavedUser Save(SavedUser user)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql =
            """
        INSERT INTO users(
            username, hashed_password, password_salt, email
        ) 
        VALUES(@Username, @HashedPassword, @PasswordSalt, @Email);
        SELECT LAST_INSERT_ID();
        """;

        user.Id = connection.ExecuteScalar<ulong>(
            sql,
            new
            {
                user.Username, user.HashedPassword, user.PasswordSalt, user.Email
            }
        );
        return user;
    }

    public static int UpdatePasswordByEmail(SavedUser user)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql =
            """
        UPDATE users SET hashed_password = @HashedPassword,
                         password_salt = @PasswordSalt WHERE email = @Email
        """;
        return connection.Execute(sql, new { user.Email, user.HashedPassword, user.PasswordSalt });
    }

    public static IEnumerable<SavedUser> SearchUserByUsername(string usernameKeyword)
    {
        using var connection = new MySqlConnection(ConnectionString);
        const string sql = """
SELECT id, username, hashed_password, password_salt, email FROM simple_chat.users WHERE username LIKE @Keyword
""";
        return connection.Query<SavedUser>(sql, new { Keyword = $"%{usernameKeyword}%" });
    }
}