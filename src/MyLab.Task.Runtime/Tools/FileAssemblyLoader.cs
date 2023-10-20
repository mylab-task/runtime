using System.Reflection;
using System.Runtime.Loader;

namespace MyLab.Task.Runtime;

class FileAssemblyLoader : IAssemblyLoader
{
    private string _asssemblyFilePath;

    public FileAssemblyLoader(string asssemblyFilePath) => _asssemblyFilePath = asssemblyFilePath;

    public Assembly Load(AssemblyLoadContext ctx)
    {
        return ctx.LoadFromAssemblyPath(_asssemblyFilePath); 
    }
}