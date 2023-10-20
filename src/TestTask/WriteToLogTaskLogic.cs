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
        _logger.Log(LogLevel.Information, default, _options.Message ?? "[empty]", null, (s, e) => s);

        return ValueTask.CompletedTask;
    }
}
