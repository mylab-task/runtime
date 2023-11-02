namespace TestTask;

public interface IRequestSender
{
    Task SendAsync(string message);
}
