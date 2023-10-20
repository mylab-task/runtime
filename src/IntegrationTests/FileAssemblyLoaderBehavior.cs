using System.Runtime.Loader;
using MyLab.Task.Runtime;

namespace IntegrationTests;

public class FileAssemblyLoaderBehavior
{
    [Fact(DisplayName = "Load dll")]
    public void ShouldLoadAssembly()
    {
        //Arrange
        string testAssemblyPath = 
            Path.GetFullPath
            (
                Path.Combine
                (
                    Directory.GetCurrentDirectory(), 
                    "../../../../TestTask/bin/Debug/net7.0/TestTask.dll"
                )
            );
            
        var loader = new FileAssemblyLoader(testAssemblyPath);
        
        var loadCtx = new AssemblyLoadContext("test", isCollectible: true);

        //Act
        var loadedA = loader.Load(loadCtx);
        var aName = loadedA?.GetName().Name;
        loadCtx.Unload();

        //Assert
        Assert.Equal("TestTask", aName);
    }
}
