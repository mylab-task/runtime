using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log.XUnit;
using MyLab.Task.Runtime;
using Xunit.Abstractions;

namespace IntegrationTests;

public class LocalFsTaskAssetProviderBehavior
{
    private ITestOutputHelper _output;

    public LocalFsTaskAssetProviderBehavior(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "Fail if asset dir not found")]
    public void ShouldFailIfAssetDirNotFound()
    {
        //Arrange
        var srv = new ServiceCollection()
            .AddLogging(l => l.AddXUnit(_output).AddFilter(ll => true))
            .AddSingleton<ITaskAssetProvider, LocalFsTaskAssetProvider>()
            .Configure<RuntimeOptions>(o => o.AssetsPath = "absent")
            .BuildServiceProvider();

        var provider = srv.GetRequiredService<ITaskAssetProvider>();
        
        //Act & Assert
        Assert.Throws<DirectoryNotFoundException>(provider.Provide);
    }

    [Fact(DisplayName = "Find asset by nested dir")]
    public void ShouldFindAssetByNestedDir()
    {
        //Arrange
        var assetDir = new DirectoryInfo("assets/foo");

        var srv = new ServiceCollection()
            .AddLogging(l => l
                .AddXUnit(_output)
                .AddFilter(ll => true)

                )
            .AddSingleton<ITaskAssetProvider, LocalFsTaskAssetProvider>()
            .Configure<RuntimeOptions>(o => o.AssetsPath = "assets")
            .BuildServiceProvider();

        var provider = srv.GetRequiredService<ITaskAssetProvider>();
        
        TaskAsset[] foundAssets;

        if(!assetDir.Exists)
            assetDir.Create();

        //Act
        try
        {
            foundAssets = provider.Provide().ToArray();
        }
        finally
        {
            assetDir.Delete(true);
        }

        //Assert
        Assert.Single(foundAssets);
        Assert.Equal("foo", foundAssets[0].Name);
    }
}