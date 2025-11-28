using MyLab.Task.RuntimeSdk;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;

namespace TestTask
{
    public class RequestSenderTaskLogic : ITaskLogic
    {
        private IRequestSender _sender;
        private RequestSenderOptions _options;

        public  RequestSenderTaskLogic(IRequestSender sender, IOptions<RequestSenderOptions> options)
        {
            _sender = sender;
            _options = options.Value;
        }

        public async ValueTask PerformAsync(TaskIterationContext iterationContext, CancellationToken cancellationToken)
        {
            await _sender.SendAsync(_options.Message ?? "[empty]");
        }
    }
}
