using SimpleChatServer.Models;
using SimpleChatServer.Persistence;

namespace SimpleChatServer.Services;

public static class UsersInChatsService
{
    public static IEnumerable<SavedUser> GetUsersByChatId(ulong chatId)
    {
        return UsersInChatsDao.GetUsersByChatId(chatId);
    }

    public static IEnumerable<Chat> GetChatsByUser(User? user)
    {
        if (user == null)
        {
            return new List<Chat>();
        }

        return UsersInChatsDao.GetChatsByUserId(user.Id);
    }

    public static string SolveAddChatForm(AddChatForm? form)
    {
        if (form == null)
        {
            return "Internet problem, please try again later";
        }

        return AddUserToChat(form.Sender, form.RequestedChat);
    }

    public static string AddUserToChat(User user, Chat? chat)
    {
        if (chat == null)
        {
            return "Internet problem, please try again later";
        }

        if (UsersInChatsDao.GetChatByUserIdAndChatId(user.Id, chat.Id) != null)
        {
            return "User is already in the chat";
        }

        UsersInChatsDao.Save(chat, user);
        return string.Empty;
    }
}