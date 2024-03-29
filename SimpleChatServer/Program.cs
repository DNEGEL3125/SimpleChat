namespace SimpleChatServer;

public abstract class Program
{
    public static void Main()
    {
        var server = new Server();
        Console.WriteLine(server.ToString());
    }
}