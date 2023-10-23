using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;

public class TaskFactory
{
    public Type LogicType { get; }
    public Type StartupType { get; }

    public TaskQualifiedName Name { get; }

    public TaskFactory(TaskQualifiedName qualName, Type taskLogicType, Type startupType)
    {
        Name = qualName ?? throw new ArgumentNullException(nameof(qualName));
        LogicType = taskLogicType ?? throw new ArgumentNullException(nameof(taskLogicType));
        StartupType = startupType ?? throw new ArgumentNullException(nameof(startupType));
    }


}
