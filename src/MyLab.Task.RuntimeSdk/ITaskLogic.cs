namespace MyLab.Task.RuntimeSdk;

/// <summary>
/// Provides task logic
/// </summary>
public interface ITaskLogic
{
    /// <summary>
    /// Performs a logic
    /// </summary>
    ValueTask PerformAsync(TaskIterationContext iterationContext, CancellationToken cancellationToken);
}
