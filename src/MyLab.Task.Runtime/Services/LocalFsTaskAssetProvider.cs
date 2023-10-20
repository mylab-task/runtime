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

    public IEnumerable<TaskAsset> Provide()
    {
        var libDir = new DirectoryInfo(_options.AssetsPath);

        if(!libDir.Exists)
        {
            throw new DirectoryNotFoundException("Assets directory not found")
                .AndFactIs("path", _options.AssetsPath);
        }

        return libDir.GetDirectories().Select(CreateTaskAsset);
    }

    private TaskAsset CreateTaskAsset(DirectoryInfo assetDir)
    {
        var fullAssemblyPath = Path.Combine(assetDir.FullName, assetDir.Name + ".dll");
        return new TaskAsset
        (
            assetDir.Name,  
            new FileAssemblyLoader(fullAssemblyPath)
        );
    }
}

