using Microsoft.Extensions.DependencyInjection;

namespace MyLab.Task.Runtime
{
    public interface ITaskServicesPostProcessing
    {
        IServiceCollection PostProcess(IServiceCollection services);
    }
}
