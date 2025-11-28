using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyLab.Task.RuntimeSdk
{
    /// <summary>
    /// Initializes Task application
    /// </summary>
    public interface ITaskStartup
    {
        /// <summary>
        /// Add custom configuration here
        /// </summary>
        void AddConfiguration(IConfigurationBuilder configBuilder);

        /// <summary>
        /// Add task logic and references here
        /// </summary>
        void AddServices(IServiceCollection services, IConfiguration configuration);
    }
}