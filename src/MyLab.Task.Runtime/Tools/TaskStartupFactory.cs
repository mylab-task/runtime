using MyLab.Log;
using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;

public class TaskStartupFactory
{
    public Type StartupType { get; }

    public TaskQualifiedName Name { get; }

    public TaskStartupFactory(TaskQualifiedName qualName, Type startupType)
    {
        Name = qualName ?? throw new ArgumentNullException(nameof(qualName));
        StartupType = startupType ?? throw new ArgumentNullException(nameof(startupType));
    }

    public ITaskStartup Create()
    {
        var obj = Activator.CreateInstance(StartupType);
        
        if(obj == null)
        {
            throw new InvalidOperationException("Can't create startup")
                .AndFactIs("type", StartupType.FullName);
        }

        return (ITaskStartup)obj;
    }
}
