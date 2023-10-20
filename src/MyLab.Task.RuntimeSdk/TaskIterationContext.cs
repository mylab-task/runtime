namespace MyLab.Task.RuntimeSdk;

/// <summary>
/// Provides access to task logic iteration context  
/// </summary>
public class TaskIterationContext
{
    /// <summary>
    /// Iteration identifier
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Date and time of iteration start
    /// </summary>
    public DateTime StartAt { get; }

    /// <summary>
    /// Iteration report. 'null' by default.
    /// </summary>
    public IterationReport? Report { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="TaskIterationContext"/>
    /// </summary>
    public TaskIterationContext(string id, DateTime startAt)
    {
        Id = id;
        StartAt = startAt;
    }
}
