namespace MyLab.Task.RuntimeSdk;

/// <summary>
/// Defines type meta-properties
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TaskAttribute : Attribute
{
    /// <summary>
    /// Task identical name
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// Startup type
    /// </summary>
    public Type? Startup { get; set; }
}
