using Microsoft.Extensions.Logging;
using MyLab.Task.RuntimeSdk;
using Microsoft.Extensions.Options;

namespace TestTask;

public class WriteToLogTaskLogic : ITaskLogic
{
    private ILogger _logger;
    private WriteToLogOptions _options;

    public  WriteToLogTaskLogic(ILogger logger, IOptions<WriteToLogOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public ValueTask PerformAsync(TaskIterationContext iterationContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation(_options.Message);

        return ValueTask.CompletedTask;
    }
}
