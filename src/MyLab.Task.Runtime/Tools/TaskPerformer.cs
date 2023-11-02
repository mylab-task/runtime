using System.Diagnostics;
using MyLab.Log;
using MyLab.Log.Scopes;
using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;

using Task = System.Threading.Tasks.Task;

class TaskPerformer : ITaskPerformer
{
    public const string TaskPerformingActivityName ="task-performing";

    private IServiceProvider _appServices;

    public ActivitySource? ActivitySource{ get; set; }

    public TaskQualifiedName TaskName { get; }

    public IProtocolWriter? ProtocolWriter{ get; set; }

    public TaskPerformer(TaskQualifiedName name, IServiceProvider appServices)
    {
        _appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
        TaskName = name ?? throw new ArgumentNullException(nameof(name));
    }

    public async Task PerformIterationAsync(CancellationToken cancellationToken)
    {
        using var scope =_appServices.CreateScope();

        var task = scope.ServiceProvider.GetService<ITaskLogic>() 
            ?? throw new InvalidOperationException("Task logic service (ITaskLogic) not found")
                .AndFactIs("task", TaskName.ToString());
        
        using var activity = ActivitySource?.StartActivity(TaskPerformingActivityName);
        string? traceId = activity?.TraceId.ToHexString();

        var ctx = new TaskIterationContext(traceId,DateTime.Now);
        IDisposable? logScope = null;

        if(scope.ServiceProvider.GetService<ILoggerFactory>() is {} logFactory)
        {
            var logger = logFactory.CreateLogger<TaskPerformer>();

            var labels = new Dictionary<string, string>
            {
                { LogLabels.TaskName, TaskName.ToString() }
            };

            if(traceId != null)
            {
                labels.Add(PredefinedLabels.TraceId, traceId);
            }

            var labelsScope = new LabelLogScope(labels);

            logScope = logger.BeginScope(labels);
        }

        try
        {
            await task.PerformAsync(ctx, cancellationToken);

            if(ProtocolWriter != null)
            {
                await ProtocolWriter.WriteAsync(ctx, ctx.StartAt - DateTime.Now);
            }
        }
        catch(Exception e)
        {
            e.AndLabel(LogLabels.TaskName, TaskName.ToString());

            if(traceId != null)
            {
                e.AndLabel(PredefinedLabels.TraceId, traceId);
            }

            if(ProtocolWriter != null)
            {
                await ProtocolWriter.WriteAsync(ctx, ctx.StartAt - DateTime.Now, e);
            }

            throw;
        }
        finally
        {
            logScope?.Dispose();
        }
    }
}
