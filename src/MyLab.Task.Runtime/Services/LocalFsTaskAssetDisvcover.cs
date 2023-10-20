using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;

namespace MyLab.Task.Runtime;

class LocalFsTaskAssetDisvcover : ITaskAssetDiscover
{
    private RuntimeOptions _options;
    private IDslLogger? _log;

    public LocalFsTaskAssetDisvcover(
        IOptions<RuntimeOptions> options,
        ILogger<LocalFsTaskAssetDisvcover>? logger = null)
    {
        _options = options.Value;
        _log = logger?.Dsl();
    }

    public IEnumerable<TaskAsset> Discover()
    {
        var libDir = new DirectoryInfo(_options.AssetPath);

        if(!libDir.Exists)
        {
            throw new DirectoryNotFoundException("Asset path not found")
                .AndFactIs("path", _options.AssetPath);
        }

        var foundAssets = new List<TaskAsset>();

        foreach(var dir in libDir.GetDirectories())
        {
            _log?.Action("Discover an asset")
                .AndFactIs("name", dir.Name)
                .AndFactIs("path", dir.FullName)
                .Write();

            var assetLibPath = Path.Combine(dir.FullName, dir.Name + ".dll");
            
            if(File.Exists(assetLibPath))
            {
                foundAssets.Add(new TaskAsset(dir.Name, assetLibPath));
            }
            else
            {
                _log?.Warning("Asset library file not found")
                    .AndFactIs("name", dir.Name)    
                    .AndFactIs("path", assetLibPath)
                    .Write(); 
            }
        }

        return foundAssets;
    }
}

