using MyLab.Log;

namespace MyLab.Task.Runtime;

static class Integration
{
    public static IServiceCollection SetToTaskRuntimeLogging(this IServiceCollection services)
    {
        return services.AddLogging
        (
            lb => lb.ClearProviders()
                .AddConsole()
                .AddMyLabConsole()
        );
    }

    public static IServiceCollection AddTaskRuntime(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        return services
            .SetToTaskRuntimeLogging()
            .AddHostedService<RuntimeLogicService>()
            .Configure<RuntimeOptions>(configuration.GetSection("Runtime"));
    }
}
