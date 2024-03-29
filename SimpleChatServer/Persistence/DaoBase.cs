using MySqlConnector;

namespace SimpleChatServer.Persistence;

public abstract class DaoBase
{
    protected const string ConnectionString = "Server=localhost; User ID=root; Password=; Database=simple_chat";
    protected static readonly MySqlConnection Db = new(ConnectionString);
}