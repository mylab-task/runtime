using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Log.XUnit;
using MyLab.Task.Runtime;
using Xunit.Abstractions;

namespace IntegrationTests;

public class LocalFsTaskAssetProviderBehavior
{
    [Fact(DisplayName = "Fail if asset dir not found")]
    public void ShouldFailIfAssetDirNotFound()
    {
        //Arrange
        ITaskAssetProvider provider = new LocalFsTaskAssetProvider("absent");
        
        //Act & Assert
        Assert.Throws<DirectoryNotFoundException>(provider.Provide);
    }

    [Fact(DisplayName = "Find asset by nested dir")]
    public void ShouldFindAssetByNestedDir()
    {
        //Arrange
        ITaskAssetProvider provider = new LocalFsTaskAssetProvider("assets");
        var assetDir = new DirectoryInfo("assets/foo");

        TaskAssetSource[] foundAssets;

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