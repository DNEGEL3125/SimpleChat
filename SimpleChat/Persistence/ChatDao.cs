using System.Collections.Generic;
using SimpleChat.Models;
using LiteDB;

namespace SimpleChat.Persistence;

public class ChatDao
{
    private const string ConnectionString = "data.db";

    private static readonly LiteDatabase Db = new(ConnectionString);
    private static ILiteCollection<Chat> ChatCollection => Db.GetCollection<Chat>("Chats");

    public static void Save(Chat chat)
    {
        ChatCollection.Insert(chat);
    }

    public static IEnumerable<Chat> GetChats(int offset = 0, int limit = 20)
    {
        // Query chat messages with pagination
        var chats = ChatCollection.Query()
            .Offset(offset)
            .Limit(limit)
            .ToList();

        return chats ?? new List<Chat>();
    }

    public static void ClearAllChats()
    {
        ChatCollection.DeleteAll();
    }

    public static void DeleteChatById(uint id)
    {
        ChatCollection.Delete(new BsonValue(id));
    }
}