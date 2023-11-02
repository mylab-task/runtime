namespace MyLab.Task.Runtime;

public interface ITaskServicesPostProcessing
{
    IServiceCollection PostProcess(IServiceCollection services);
}
