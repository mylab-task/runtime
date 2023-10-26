namespace MyLab.Task.Runtime;

using Task = System.Threading.Tasks.Task;


interface ITaskPerformer
{
    TaskQualifiedName TaskName { get; }
    Task PerformIterationAsync(CancellationToken cancellationToken);
}