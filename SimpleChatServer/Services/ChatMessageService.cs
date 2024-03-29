using SimpleChatServer.Models;
using SimpleChatServer.Persistence;

namespace SimpleChatServer.Services;

public static class ChatMessageService
{
    public static void SaveMessage(ChatMessage<string>? chatMessage)
    {
        if (chatMessage == null)
        {
            return;
        }

        chatMessage.Id = ChatMessageDao.SaveAndGetId(chatMessage);
    }

    public static IEnumerable<ChatMessage<string>> SolveLoadMoreMessageRequest(LoadMoreChatMessagesForm? lastMessage)
    {
        if (lastMessage == null)
        {
            return new List<ChatMessage<string>>();
        }

        return ChatMessageDao
            .GetChatMessagesByForm(
                lastMessage
            );
    }
}