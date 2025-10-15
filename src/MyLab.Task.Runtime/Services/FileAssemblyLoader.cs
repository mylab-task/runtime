using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace MyLab.Task.Runtime
{
    class FileAssemblyLoader : IAssemblyLoader
    {
        private string _assemblyFilePath;
        private string _assemblyDirectoryName;

        public FileAssemblyLoader(string assemblyFilePath)
        {
            _assemblyFilePath = assemblyFilePath;
            _assemblyDirectoryName = Path.GetDirectoryName(_assemblyFilePath)!;
        }

        public Assembly Load(AssemblyLoadContext ctx)
        {
            var targetAssembly =  ctx.LoadFromAssemblyPath(_assemblyFilePath);
            ctx.Resolving += ExtAssemblyResolving;
            return targetAssembly;
        }

        private Assembly? ExtAssemblyResolving(AssemblyLoadContext ctx, AssemblyName assemblyName)
        {
            var foundDlls = Directory.GetFileSystemEntries(new FileInfo(_assemblyDirectoryName).FullName, assemblyName.Name + ".dll", SearchOption.AllDirectories);
            if (foundDlls.Any())
            {
                return ctx.LoadFromAssemblyPath(foundDlls[0]);
            }
            return null;
        }
    }
}