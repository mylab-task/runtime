using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Task.RuntimeSdk;

namespace TestOldTask
{
    public class TestTaskStartup : ITaskStartup
    {
        public void AddConfiguration(IConfigurationBuilder configBuilder)
        {
        
        }

        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITaskLogic, TestTaskLogic>();
        }
    }
}
