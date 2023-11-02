
namespace MyLab.Task.Runtime;

class TaskServicesPostProcessing : ITaskServicesPostProcessing
{
    public IServiceCollection PostProcess(IServiceCollection services)
    {
        return services.SetToTaskRuntimeLogging();
    }
}
