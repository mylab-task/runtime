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
            .ToArray();

        if(found.Length == 0) 
        {
            throw new InvalidOperationException("No tasks found");
        }
        if(found.Count(t => string.IsNullOrWhiteSpace(t.Name)) > 1)
        {
            throw new InvalidOperationException("More then one task with default name detected");
        } 

        foreach(var t in found)
        {
            if(t.HasGenericParams)
            {
                throw new InvalidOperationException("Generic startup types is not supported")
                    .AndFactIs("type", t.Type.FullName);
            }
            if(!t.HasPubDefCtor)
            {
                throw new InvalidOperationException("Startup type has no default public constructor")
                    .AndFactIs("type", t.Type.FullName);
            }
        }

        return found.Select(t => new TaskFactory
            (
                new TaskQualifiedName(_taskAssetSource.Name, t.Name),
                t.Type
            )
        );
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

    public record TypeDesc
    (
        Type Type, 
        bool IsPublic, 
        bool IsImplTaskStartup,
        bool HasGenericParams, 
        bool HasPubDefCtor, 
        string? Name
    )
    {
        public static TypeDesc FromType(Type type)
        {
            return new TypeDesc
            (
                type,
                type.IsPublic,
                type.IsAssignableTo(typeof(ITaskStartup)),
                type.GetGenericArguments().Length != 0,
                type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0),
                type.GetCustomAttribute<TaskNameAttribute>()?.Name
            );
        }
    }
}