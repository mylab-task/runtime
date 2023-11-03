using System.Runtime.Loader;
using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;
using Xunit;

namespace UnitTests
{
    public class FileAssemblyLoaderBehavior
    {
        [Fact(DisplayName = "Should load dll")]
        public void ShouldLoadAssembly()
        {
            //Arrange
            var loader = new FileAssemblyLoader(TestStuff.GetTestAssemblyPath());
        
            var loadCtx = new AssemblyLoadContext("test", isCollectible: true);

            //Act
            var loadedA = loader.Load(loadCtx);

            //Assert
            Assert.NotNull(loadedA);
            Assert.Equal("TestTask", loadedA.GetName().Name);
            Assert.Contains(loadedA.DefinedTypes, t => t.IsAssignableTo(typeof(ITaskStartup)));
        }

        [Fact(DisplayName = "Should load dll with old SDK")]
        public void ShouldNotLoadBadAssetAssembly()
        {
            //Arrange
            var loader = new FileAssemblyLoader(TestStuff.GetOldRefTestAssemblyPath());
        
            var loadCtx = new AssemblyLoadContext("test", isCollectible: true);

            //Act
            var loadedA = loader.Load(loadCtx);
        
            //Assert
            Assert.NotNull(loadedA);
            Assert.Equal("TestOldTask", loadedA.GetName().Name);
            Assert.Contains(loadedA.DefinedTypes, t => t.IsAssignableTo(typeof(ITaskStartup)));
        }
    }
}
