namespace SimpleChat.Models;

public class ChatImage
{
    public string FilePath { get; set; } = string.Empty;
    public string Base64Data { get; set; } = string.Empty;

    public string ToMarkDown(string? username = null)
    {
        return username == null
            ? $"""<img src="{FilePath}">"""
            : $"""<img alt="{username}'s image" src="{FilePath}">""";
    }
}