using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log;

namespace MyLab.Task.Runtime
{
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

        public static IServiceCollection AddTaskRuntime(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            return services
                .SetToTaskRuntimeLogging()
                .AddHostedService<RuntimeLogicService>()
                .AddSingleton<IProtocolWriter, ProtocolWriter>()
                .AddSingleton<ITaskServicesPostProcessing, TaskServicesPostProcessing>();
        }

        public static IServiceCollection ConfigureTaskRuntime(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));

            return services
                .Configure<RuntimeOptions>(configuration.GetSection("Runtime"));
        }

        public static IServiceCollection ConfigureTaskRuntime(this IServiceCollection services, Action<RuntimeOptions> configureOptions)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configureOptions is null) throw new ArgumentNullException(nameof(configureOptions));

            return services
                .Configure<RuntimeOptions>(configureOptions);
        }
    }
}
