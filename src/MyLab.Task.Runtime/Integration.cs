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
}
