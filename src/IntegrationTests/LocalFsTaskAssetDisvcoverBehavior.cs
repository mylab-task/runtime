using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log.XUnit;
using MyLab.Task.Runtime;
using Xunit.Abstractions;

namespace IntegrationTests;

public class LocalFsTaskAssetDisvcoverBehavior
{
    private ITestOutputHelper _output;

    public LocalFsTaskAssetDisvcoverBehavior(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "Fail if asset dir not found")]
    public void ShouldFailIfAssetDirNotFound()
    {
        //Arrange
        var srv = new ServiceCollection()
            .AddLogging(l => l.AddXUnit(_output).AddFilter(ll => true))
            .AddSingleton<ITaskAssetDiscover, LocalFsTaskAssetDisvcover>()
            .Configure<RuntimeOptions>(o => o.AssetPath = "absent")
            .BuildServiceProvider();

        var discover = srv.GetRequiredService<ITaskAssetDiscover>();
        
        //Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => discover.Discover());
    }

    [Fact(DisplayName = "Fail if asset lib not found")]
    public void ShouldIgnoreIfAssetLibNotFound()
    {
        //Arrange
        var assetDir = new DirectoryInfo("assets/empty");

        var srv = new ServiceCollection()
            .AddLogging(l => l.AddXUnit(_output).AddFilter(ll => true))
            .AddSingleton<ITaskAssetDiscover, LocalFsTaskAssetDisvcover>()
            .Configure<RuntimeOptions>(o => o.AssetPath = "assets")
            .BuildServiceProvider();

        var discover = srv.GetRequiredService<ITaskAssetDiscover>();
        
        TaskAsset[] foundAssets;

        if(!assetDir.Exists)
            assetDir.Create();

        //Act
        try
        {
            foundAssets = discover.Discover().ToArray();
        }
        finally
        {
            assetDir.Delete(true);
        }

        //Assert
        Assert.Empty(foundAssets);
    }

    [Fact(DisplayName = "Find asset")]
    public void ShouldFindAsset()
    {
        //Arrange
        var assetDir = new DirectoryInfo("assets/foo");

        var srv = new ServiceCollection()
            .AddLogging(l => l
                .AddXUnit(_output)
                .AddFilter(ll => true)

                )
            .AddSingleton<ITaskAssetDiscover, LocalFsTaskAssetDisvcover>()
            .Configure<RuntimeOptions>(o => o.AssetPath = "assets")
            .BuildServiceProvider();

        var discover = srv.GetRequiredService<ITaskAssetDiscover>();
        
        TaskAsset[] foundAssets;

        if(!assetDir.Exists)
            assetDir.Create();
        File.Create("assets/foo/foo.dll");

        //Act
        try
        {
            foundAssets = discover.Discover().ToArray();
        }
        finally
        {
            assetDir.Delete(true);
        }

        //Assert
        Assert.Single(foundAssets);
        Assert.Equal("foo", foundAssets[0].Name);
        Assert.EndsWith("assets/foo/foo.dll", foundAssets[0].LibPath);
    }
}