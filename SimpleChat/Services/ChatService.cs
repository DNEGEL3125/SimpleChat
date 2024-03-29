using System.Collections.Generic;
using SimpleChat.Models;
using SimpleChat.Persistence;

namespace SimpleChat.Services;

public static class ChatService
{
    public static IEnumerable<Chat> GetChatList()
    {
        return ChatDao.GetChats();
    }

    public static void SaveChat(Chat chat)
    {
        ChatDao.Save(chat);
    }
}