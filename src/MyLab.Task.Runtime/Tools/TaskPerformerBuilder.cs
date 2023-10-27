using System.Diagnostics;
using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;

class TaskPerformerBuilder
{
    TaskQualifiedName _name;
    ITaskStartup _startup; 
    public IConfiguration? BaseConfig { get; set; }
    public Action<IServiceCollection>? PostServiceProc { get; set; }
    public ActivitySource? ActivitySource { get; set; }

    public TaskPerformerBuilder(TaskQualifiedName name, ITaskStartup startup)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _startup = startup ?? throw new ArgumentNullException(nameof(startup));
    }

    public ITaskPerformer Build()
    {
        var configBuilder = new ConfigurationBuilder();
        if(BaseConfig != null) configBuilder.AddConfiguration(BaseConfig);

        _startup.AddConfiguration(configBuilder);

        var config = configBuilder.Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config);
        
        _startup.AddServices(services, config);

        PostServiceProc?.Invoke(services);

        var serviceProvider = services.BuildServiceProvider();

        return new TaskPerformer(_name, serviceProvider)
        {
            ActivitySource = ActivitySource
        };
    }
}
