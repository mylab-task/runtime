using Microsoft.Extensions.Options;
using MyLab.Log;

namespace MyLab.Task.Runtime;

class LocalFsTaskAssetProvider : ITaskAssetProvider
{
    private RuntimeOptions _options;

    public LocalFsTaskAssetProvider(
        IOptions<RuntimeOptions> options)
    {
        _options = options.Value;
    }

    public IEnumerable<TaskAssetSource> Provide()
    {
        var libDir = new DirectoryInfo(_options.AssetsPath);

        if(!libDir.Exists)
        {
            throw new DirectoryNotFoundException("Assets directory not found")
                .AndFactIs("path", _options.AssetsPath);
        }

        return libDir.GetDirectories().Select(CreateTaskAsset);
    }

    private TaskAssetSource CreateTaskAsset(DirectoryInfo assetDir)
    {
        var fullAssemblyPath = Path.Combine(assetDir.FullName, assetDir.Name + ".dll");
        return new TaskAssetSource
        (
            assetDir.Name,  
            new FileAssemblyLoader(fullAssemblyPath)
        );
    }
}

