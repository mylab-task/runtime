using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Task.RuntimeSdk;

namespace TestTask
{
    public class RequestSenderTaskStartup : ITaskStartup
    {
        public void AddConfiguration(IConfigurationBuilder configBuilder)
        {
        
        }

        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSingleton<ITaskLogic, RequestSenderTaskLogic>()
                .Configure<RequestSenderOptions>(configuration.GetSection(RequestSenderOptions.SectionName));
        }
    }
}
