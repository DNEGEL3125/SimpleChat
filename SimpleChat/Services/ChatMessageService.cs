using System.Collections.Generic;
using LiteDB;
using SimpleChat.Models;

namespace SimpleChat.Services;

public static class ChatMessageService
{
    private const string ConnectionString = "data.db";

    private static readonly LiteDatabase Db = new(ConnectionString);
    private static ILiteCollection<ChatMessage> ChatMessageCollection => Db.GetCollection<ChatMessage>("ChatMessages");

    public static IEnumerable<ChatMessage> GetChatMessagesByChatId(ulong chatId, int limit = 30, int offset = 0)
    {
        return ChatMessageCollection.Query()
            .Where(x => x.ChatId == chatId)
            .OrderByDescending(x => x.SendDateTime)
            .Offset(offset).Limit(limit).ToList();
    }

    public static void Save(ChatMessage chatMessage)
    {
        ChatMessageCollection.Insert(chatMessage);
    }

    public static void Save(IEnumerable<ChatMessage> chatMessages)
    {
        ChatMessageCollection.Insert(chatMessages);
    }
}