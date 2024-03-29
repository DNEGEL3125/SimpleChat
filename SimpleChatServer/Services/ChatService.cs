using SimpleChatServer.Models;
using SimpleChatServer.Persistence;

namespace SimpleChatServer.Services;

public static class ChatService
{
    public static Chat? CreateChat(CreateChatForm? form)
    {
        if (form == null)
        {
            return null;
        }

        var chat = ChatDao.CreateChatByTitle(form.ChatTitle);
        UsersInChatsDao.Save(chat, form.Members);
        return chat;
    }
}