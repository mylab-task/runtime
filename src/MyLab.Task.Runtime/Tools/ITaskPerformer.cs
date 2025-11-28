namespace MyLab.Task.Runtime
{
    using System.Threading;
    using Task = System.Threading.Tasks.Task;


    public interface ITaskPerformer
    {
        TaskQualifiedName TaskName { get; }
        Task PerformIterationAsync(CancellationToken cancellationToken);
    }
}