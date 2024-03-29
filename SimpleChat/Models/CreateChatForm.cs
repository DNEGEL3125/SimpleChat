using System.Collections.Generic;

namespace SimpleChat.Models;

public class CreateChatForm
{
    public IEnumerable<User> Members { get; set; } = new List<User>();
    public string ChatTitle { get; set; } = string.Empty;
}