using Dapper;
using SimpleChatServer.Models;

namespace SimpleChatServer.Persistence;

public abstract class ChatMessageDao : DaoBase
{
    /// <summary>
    /// Get ChatMessages with id smaller than chatMessageId and
    /// closest to chatMessageId
    /// </summary>
    /// <param name="form">Form contains request info</param>
    /// <returns></returns>
    public static IEnumerable<ChatMessage<string>> GetChatMessagesByForm(
        LoadMoreChatMessagesForm form
    )
    {
        const string sql =
            "SELECT cm.id, cm.chat_id AS ChatId, cm.content, cm.type," +
            "cm.send_date_time AS SendDateTime, u.username, u.email, u.id " +
            "FROM chat_messages AS cm " +
            "INNER JOIN users AS u ON cm.sender_id = u.id " +
            "WHERE cm.chat_id = @ChatId AND cm.id < @ChatMessageIdSmaller " +
            "ORDER BY cm.id DESC LIMIT @QueryLimit";
        return Db.Query<ChatMessage<string>, User, ChatMessage<string>>(
            sql,
            (message, user) =>
            {
                message.Sender = user;
                return message;
            },
            new
            {
                form.ChatId,
                form.ChatMessageIdSmaller,
                QueryLimit = form.Count
            },
            splitOn: "username"
        );
    }


    public static ulong SaveAndGetId(ChatMessage<string> chatMessage)
    {
        const string sql =
            """
            INSERT INTO chat_messages(
                chat_id, content, sender_id, send_date_time
            )
            WITH SenderCTE AS ( 
                SELECT id AS user_id FROM users WHERE email = @Email
            ) SELECT @ChatId, @Content, user_id, @SendDateTime FROM SenderCTE;
            SELECT LAST_INSERT_ID();
        """;
        return Db.ExecuteScalar<ulong>(sql,
            new
            {
                chatMessage.ChatId,
                chatMessage.Content,
                chatMessage.SendDateTime,
                chatMessage.Sender.Email
            });
    }

    public static ulong SaveAndGetId(ChatMessage<ChatImage> chatMessage, string filePath)
    {
        const string sql = """
        INSERT INTO chat_messages(
        chat_id, content, sender_id, send_date_time, type
        ) VALUES (@ChatId, @Content, @SenderId, @SendDateTime, @Type);
        SELECT LAST_INSERT_ID();
        """;
        return Db.ExecuteScalar<ulong>(
            sql,
            new
            {
                chatMessage.ChatId,
                Content = filePath,
                SenderId = chatMessage.Sender.Id,
                chatMessage.SendDateTime,
                Type = ChatMessageType.Image
            }
        );
    }
}