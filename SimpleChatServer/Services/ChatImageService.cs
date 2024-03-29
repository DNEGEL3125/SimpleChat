using SimpleChatServer.Models;
using SimpleChatServer.Persistence;

namespace SimpleChatServer.Services;

public static class ChatImageService
{
    public static void SolveChatImage(ChatMessage<ChatImage>? chatMessage)
    {
        if (chatMessage?.Content is not { } chatImage)
            return;

        var fileExtension = Path.GetExtension(chatImage.FilePath);

        switch (fileExtension.ToLower())
        {
            case ".jpg":
            case ".png":
            case ".gif":
                break;
            default:
                // Unsupported image type
                return;
        }

        var filePath =
            $"./Storage/{chatMessage.ChatId}/{chatMessage.SendDateTime
                .ToBinary()}/{chatMessage.Sender.Id}{fileExtension}";
        var imageData = Convert.FromBase64String(chatImage.Base64Data);
        // Create the directory if it doesn't exist
        Directory.CreateDirectory(
            Path.GetDirectoryName(filePath) ?? string.Empty
        );
        // SaveAndGetId file to directory
        File.WriteAllBytes(filePath, imageData);

        // SaveAndGetId to database
        chatMessage.Id = ChatMessageDao.SaveAndGetId(chatMessage, filePath);
        chatImage.FilePath = filePath;
    }
}