using System.Data;
using System.Diagnostics;
using MyLab.Log;
using MyLab.Log.Scopes;
using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;
class TaskApp
{
    private IServiceProvider _appServices;

    public TaskQualifiedName Name { get; }

    public TaskApp(TaskQualifiedName name, IServiceProvider appServices)
    {
        _appServices = appServices ?? throw new ArgumentNullException(nameof(appServices));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public static TaskApp Create
    (
        TaskQualifiedName name, 
        ITaskStartup startup, 
        IConfiguration? baseConfig,
        Action<IServiceCollection>? postServiceProc
    )
    {
        if (startup is null) throw new ArgumentNullException(nameof(startup));

        var configBuilder = new ConfigurationBuilder();
        if(baseConfig != null) configBuilder.AddConfiguration(baseConfig);

        startup.AddConfiguration(configBuilder);

        var config = configBuilder.Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config);
        
        startup.AddServices(services, config);

        postServiceProc?.Invoke(services);

        var serviceProvider = services.BuildServiceProvider();

        return new TaskApp(name, serviceProvider);
    }

    public async System.Threading.Tasks.Task PerformIterationAsync(CancellationToken cancellationToken)
    {
        using var scope =_appServices.CreateScope();

        var task = scope.ServiceProvider.GetService<ITaskLogic>() 
            ?? throw new InvalidOperationException("Task logic service (ITaskLogic) not found")
                .AndFactIs("task", Name.ToString());
        
        string? traceId = Activity.Current?.TraceId.ToHexString();

        var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
        IDisposable? logScope = null;
        if(loggerFactory != null)
        {
            var logger = loggerFactory.CreateLogger<TaskApp>();
            
            var labels = new Dictionary<string, string>
            {
                { LogScopes.TaskNameFact, Name.ToString() }
            };
            
            if(traceId != null)
                labels.Add(PredefinedLabels.TraceId, traceId);

            logScope = logger.BeginScope
            (
                new LabelLogScope(labels)
            );
        }

        try
        {
            var ctx = new TaskIterationContext(traceId,DateTime.Now);
            await task.PerformAsync(ctx, cancellationToken);
        }
        finally
        {
            logScope?.Dispose();
        }
    }
}
