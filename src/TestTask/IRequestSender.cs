using System.Threading.Tasks;

namespace TestTask
{
    public interface IRequestSender
    {
        Task SendAsync(string message);
    }
}
