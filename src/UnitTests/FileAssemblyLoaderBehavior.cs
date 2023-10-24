using System.Runtime.Loader;
using MyLab.Task.Runtime;

namespace UnitTests;

public class FileAssemblyLoaderBehavior
{
    [Fact(DisplayName = "Load dll")]
    public void ShouldLoadAssembly()
    {
        //Arrange
        var loader = new FileAssemblyLoader(TestStuff.GetTestAssemblyPath());
        
        var loadCtx = new AssemblyLoadContext("test", isCollectible: true);

        //Act
        var loadedA = loader.Load(loadCtx);
        var aName = loadedA?.GetName().Name;

        //Assert
        Assert.Equal("TestTask", aName);
    }
}
