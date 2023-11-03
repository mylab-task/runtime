using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;

namespace MyLab.Task.Runtime
{
    using Task = System.Threading.Tasks.Task;

    class RuntimeLogicService : BackgroundService
    {
        private IDslLogger? _log;
        private RuntimeOptions _options;
        private IConfiguration _config;
        private ITaskServicesPostProcessing _taskServicesPostProcessing;
        private ITaskAssetProvider _taskAssetProvider;

        private TaskConfigBuilder _taskConfigBuilder;

        public RuntimeLogicService
        (
            ITaskAssetProvider taskAssetProvider, 
            ITaskServicesPostProcessing taskServicesPostProcessing,
            IOptions<RuntimeOptions> options,
            IConfiguration config,
            ILogger<RuntimeLogicService>? logger = null
        )
        {
            _log = logger?.Dsl();
            _options = options.Value;
            _config = config;
            _taskServicesPostProcessing = taskServicesPostProcessing;
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

            Scheduler scheduler;
        
            try
            {
                scheduler = new Scheduler(TimeSpan.FromSeconds(1))
                {
                    Logger = _log
                };
                RegisterTasks(taskPerformers, scheduler);
            }
            catch(Exception e)
            {
                _log?.Error("Unable to register tasks in the scheduler", e).Write();
                return;
            }

            try
            {
                await scheduler.RunAsync(stoppingToken);
            }
            catch(Exception e)
            {
                _log?.Error("Unable to run scheduler", e).Write();
                return;
            }
        }

        private void RegisterTasks(IEnumerable<ITaskPerformer> taskPerformers, Scheduler scheduler)
        {
        
            foreach (var p in taskPerformers)
            {
                using(_log?.BeginScope(new LabelLogScope(LogLabels.TaskName, p.TaskName.ToString())))
                {
                    if(_options.Tasks == null || !_options.Tasks.TryGetValue(p.TaskName.ToString(), out var tOpts))
                    {
                        _log?.Warning("Task config not found").Write();
                        continue;
                    }

                    if(!tOpts.Period.HasValue || tOpts.Period.Value == default)
                    {
                        _log?.Warning("Task period is not specified").Write();
                        continue;
                    }

                    scheduler.RegisterTask(p, tOpts.Period.Value);

                    _log?.Action("The task has been registered in scheduler")
                        .AndFactIs("period", tOpts.Period.Value)
                        .Write();
                }
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
                    ActivitySource = new ActivitySource(TraceActivitySourceNames.Default),
                    BaseConfig = _taskConfigBuilder.Build(f.Name.ToString()),
                    PostServiceProc = s => _taskServicesPostProcessing.PostProcess(s)
                };

                var performer = performerBuilder.Build();

                yield return performer;
            }
        }
    }
}
