using System.Diagnostics;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;

namespace MyLab.Task.Runtime;

using Task = System.Threading.Tasks.Task;

class RuntimeLogicService : BackgroundService
{
    private IDslLogger? _log;
    private RuntimeOptions _options;
    private IConfiguration _config;
    private ITaskAssetProvider _taskAssetProvider;

    private TaskConfigBuilder _taskConfigBuilder;

    public RuntimeLogicService
    (
        ITaskAssetProvider taskAssetProvider, 
        IOptions<RuntimeOptions> options,
        IConfiguration config,
        ILogger<RuntimeLogicService>? logger = null
    )
    {
        _log = logger?.Dsl();
        _options = options.Value;
        _config = config;
        _taskAssetProvider = taskAssetProvider ?? throw new ArgumentNullException(nameof(taskAssetProvider));
        _taskConfigBuilder = new TaskConfigBuilder(_config, _options);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IEnumerable<ITaskPerformer> taskPerformers;
        try
        {
            taskPerformers = LoadTaskAssets();
        }
        catch (Exception e)
        {
            _log?.Error("Unable to load task assets", e).Write();
            return;
        }

        var scheduler = new Scheduler(TimeSpan.FromSeconds(1));

        RegisterTasks(taskPerformers, scheduler);

        await scheduler.RunAsync(stoppingToken);
    }

    private void RegisterTasks(IEnumerable<ITaskPerformer> taskPerformers, Scheduler scheduler)
    {
        foreach (var p in taskPerformers)
        {
            var period = _options.Tasks != null && _options.Tasks.TryGetValue(p.TaskName.ToString(), out var tOpts)
                ? (tOpts.Period ?? _options.DefaultPeriod)
                : _options.DefaultPeriod;

            scheduler.RegisterTask(p, period);
        }
    }

    private IEnumerable<ITaskPerformer> LoadTaskAssets()
    {
        var taskAssetSources = _taskAssetProvider.Provide();

        var taskPerformers = new List<ITaskPerformer>();
        foreach (var assetSrc in taskAssetSources)
        {
            using (var assetScope = _log?.BeginScope(new LabelLogScope("asset", assetSrc.Name)))
            {
                try
                {
                    var assetPerformers = LoadAsset(assetSrc);
                    taskPerformers.AddRange(assetPerformers);

                    _log?.Action("Task asset has been loaded")
                        .AndFactIs("task-list", string.Join(", ", assetPerformers.Select(p => p.TaskName.ToString())))
                        .Write();
                }
                catch (Exception e)
                {
                    _log.Error("Unable to load task asset", e).Write();
                }
            }
        }

        return taskPerformers;
    }

    IEnumerable<ITaskPerformer> LoadAsset(TaskAssetSource assetSrc)
    {
        var extractor = new TaskAssetExtractor(assetSrc);
            
        IEnumerable<TaskStartupFactory> taskFactories;
        
        taskFactories = extractor.Extract();

        foreach (var f in taskFactories)
        {
            RuntimeSdk.ITaskStartup startup;
            try
            {
                startup = f.Create();
            }
            catch (Exception e)
            {
                _log?.Error("Unable to create task startup", e)
                    .AndFactIs("task", f.Name)
                    .Write();
                continue;
            }

            var performerBuilder = new TaskPerformerBuilder(f.Name, startup)
            {
                ActivitySource = new ActivitySource(Constants.TraceActivitySourceName),
                BaseConfig = _taskConfigBuilder.Build(f.Name.ToString()),
                PostServiceProc = s => s.SetToTaskRuntimeLogging()
            };

            var performer = performerBuilder.Build();

            yield return performer;
        }
    }
}
