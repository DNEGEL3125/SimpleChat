namespace SimpleChatServer.Services;

public class ClientService
{
    private ClientService()
    {
    }

    public static ClientService Instance { get; } = new();
}