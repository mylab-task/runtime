using Microsoft.Extensions.Logging;
using MyLab.Task.RuntimeSdk;

namespace TestTask;

public class WriteToLogTaskLogic : ITaskLogic
{
    private ILogger _logger;

    public  WriteToLogTaskLogic(ILogger logger)
    {
        _logger = logger;
    }

    public ValueTask PerformAsync(TaskIterationContext iterationContext, CancellationToken cancellationToken)
    {
        
    }
}
