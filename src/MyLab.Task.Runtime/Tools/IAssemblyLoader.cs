using System.Reflection;
using System.Runtime.Loader;

namespace MyLab.Task.Runtime;

interface IAssemblyLoader
{
    Assembly Load(AssemblyLoadContext ctx);
}