using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;

public class TaskFactory
{
    public Type StartupType { get; }

    public TaskQualifiedName Name { get; }

    public TaskFactory(TaskQualifiedName qualName, Type startupType)
    {
        Name = qualName ?? throw new ArgumentNullException(nameof(qualName));
        StartupType = startupType ?? throw new ArgumentNullException(nameof(startupType));
    }


}
