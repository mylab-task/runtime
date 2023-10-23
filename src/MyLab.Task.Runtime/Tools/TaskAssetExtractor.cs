using System.Reflection;
using System.Runtime.Loader;
using MyLab.Log;
using MyLab.Task.RuntimeSdk;

namespace MyLab.Task.Runtime;

public class TaskAssetExtractor
{
    private TaskAssetSource _taskAssetSource;
    private IAssemblyTypesProvider _typesProvider;

    public TaskAssetExtractor(TaskAssetSource taskAssetSource)
        :this(taskAssetSource, new DefaultIAssemblyTypesProvider())
    {
    }

    public TaskAssetExtractor(TaskAssetSource taskAssetSource, IAssemblyTypesProvider typesProvider)
    {
        _taskAssetSource = taskAssetSource ?? throw new ArgumentNullException(nameof(taskAssetSource));
        _typesProvider = typesProvider ?? throw new ArgumentNullException(nameof(typesProvider));
    }

    public IEnumerable<TaskFactory> Extract()
    {
        var ctx = new AssemblyLoadContext("task:" + _taskAssetSource.Name);

        var assembly =_taskAssetSource.Loader.Load(ctx);
        
        var taskLogicTypes = _typesProvider.GetTaskLogicTypes(assembly);

        Type? defaultTaskStartup = _typesProvider.GetDefaultStartupType(assembly);

        var result = new List<TaskFactory>();
        foreach (var task in taskLogicTypes)
        {
            result.Add(new TaskFactory
            (
                new TaskQualifiedName(_taskAssetSource.Name, task.Attribute?.Name),
                task.LogicType,
                DetermineStartup(task.LogicType.Name, task.Attribute?.Startup, defaultTaskStartup)
            ));
        }

        if(result.Count == 0)
            throw new InvalidOperationException("Tasks not found in assert")
                .AndFactIs("asset-name", _taskAssetSource.Name);

        return result;
    }

    Type DetermineStartup(string taskLogicClassName, Type? expliciteStartup, Type? defaultStartup)
    {
        if(expliciteStartup != null)
        {
            return expliciteStartup;
        }
        else
        {
            if(defaultStartup != null)
            {
                return defaultStartup;
            }
            else
            {
                throw new InvalidOperationException
                (
                    "A task should reference to a startup or there is should be the one startup in assembly"
                )
                .AndFactIs("task-logic", taskLogicClassName);
            }
        }
    }

    class DefaultIAssemblyTypesProvider : IAssemblyTypesProvider
    {
        public Type? GetDefaultStartupType(Assembly assembly)
        {
            return assembly.GetTypes().SingleOrDefault(t => t.IsAssignableTo(typeof(ITaskStartup)));
        }

        public IEnumerable<TaskLogicTypeDesc> GetTaskLogicTypes(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ITaskLogic)))
                .Select(t => new TaskLogicTypeDesc(t, t.GetCustomAttribute<TaskAttribute>()));
        }
    }

    public interface IAssemblyTypesProvider
    {
        IEnumerable<TaskLogicTypeDesc> GetTaskLogicTypes(Assembly assembly);

        Type? GetDefaultStartupType(Assembly assembly);
    }

    public record TaskLogicTypeDesc(Type LogicType, TaskAttribute? Attribute);
}