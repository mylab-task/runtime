using System.Reflection;
using System.Runtime.Loader;

namespace MyLab.Task.Runtime;

public interface IAssemblyLoader
{
    Assembly Load(AssemblyLoadContext ctx);
}