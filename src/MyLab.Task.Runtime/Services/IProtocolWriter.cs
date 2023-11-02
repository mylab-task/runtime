namespace MyLab.Task.Runtime;

using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.ProtocolStorage.Client;
using MyLab.Task.RuntimeSdk;
using Task = System.Threading.Tasks.Task;

interface IProtocolWriter
{
    Task WriteAsync(TaskIterationContext taskIterationContext, TimeSpan iterationDuration, Exception? error = null);
}

class ProtocolWriter : IProtocolWriter
{
    private SafeProtocolIndexerV1? _safeProtocolApi;
    private IDslLogger? _log;
    private string _protocolId;

    public ProtocolWriter(
        IOptions<RuntimeOptions> options,
        IProtocolApiV1? protocolApiV1 = null, 
        ILogger<ProtocolWriter>? logger = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if(protocolApiV1 != null)
        {
            _safeProtocolApi = new SafeProtocolIndexerV1(protocolApiV1, _log);
        }
        
        _log = logger?.Dsl();
        _protocolId = options.Value.ProtocolId ?? ProtocolTypes.Iterations;
    }

    public Task WriteAsync(TaskIterationContext ctx, TimeSpan iterationDuration, Exception? error = null)
    {
        if(_safeProtocolApi == null) return Task.CompletedTask;

        return _safeProtocolApi.PostEventAsync
        (   
            _protocolId,
            new TaskIterationProtocolEvent
            {
                Id = ctx.Report?.IterationId,
                DateTime = ctx.StartAt,
                Metrics = ctx.Report?.Metrics,
                Subject = ctx.Report?.SubjectId,
                Type = ProtocolTypes.Iterations,
                TraceId = ctx.TraceId,
                Workload = ctx.Report?.Workload ?? IterationWorkload.Undefined,
                Duration = iterationDuration,
                Error = error
            }
        );
    }
}
