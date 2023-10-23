using System.Reflection;
using System.Runtime.Loader;
using MyLab.Log;
using MyLab.Task.RuntimeSdk;
using YamlDotNet.Serialization.EventEmitters;

namespace MyLab.Task.Runtime;

public class TaskAssetExtractor
{
    private TaskAssetSource _taskAssetSource;
    private IStrategy _strategy;

    public TaskAssetExtractor(TaskAssetSource taskAssetSource)
        :this(taskAssetSource, new DefaultStrategy())
    {
    }

    public TaskAssetExtractor(TaskAssetSource taskAssetSource, IStrategy strategy)
    {
        _taskAssetSource = taskAssetSource ?? throw new ArgumentNullException(nameof(taskAssetSource));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    public IEnumerable<TaskFactory> Extract()
    {
        var ctx = new AssemblyLoadContext("task:" + _taskAssetSource.Name);

        var assembly =_taskAssetSource.Loader.Load(ctx);
        
        var found = _strategy.GetAssemblyTypes(assembly)
            .Where(t => t.IsPublic && t.IsImplTaskStartup)
            .Select(t => new TaskFactory
            (
                new TaskQualifiedName(_taskAssetSource.Name, t.Name),
                t.Type
            ))
            .ToArray();

        if(found.Length == 0) 
        {
            throw new InvalidOperationException("No tasks found")
                .AndFactIs("assert", _taskAssetSource.Name);
        }
        if(found.Count(t => string.IsNullOrWhiteSpace(t.Name.LocalName)) > 1)
        {
            throw new InvalidOperationException("More then one task with default name detected")
                .AndFactIs("assert", _taskAssetSource.Name);
        } 

        return found;
    }

    class DefaultStrategy : IStrategy
    {
        public IEnumerable<TypeDesc> GetAssemblyTypes(Assembly assembly)
        {
            return assembly.GetTypes().Select(t => TypeDesc.FromType(t));
        }
    }

    public interface IStrategy
    {
        IEnumerable<TypeDesc> GetAssemblyTypes(Assembly assembly);
    }

    public record TypeDesc(Type Type, bool IsPublic, bool IsImplTaskStartup, string? Name)
    {
        public static TypeDesc FromType(Type type)
        {
            return new TypeDesc
            (
                type,
                type.IsPublic,
                type.IsAssignableTo(typeof(ITaskStartup)),
                type.GetCustomAttribute<TaskNameAttribute>()?.Name
            );
        }
    }
}