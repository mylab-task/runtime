using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;
using TypeDesc = MyLab.Task.Runtime.TaskAssetExtractor.TypeDesc;

namespace IntegrationTests;

public partial class TaskAssetExtractorBehavior
{
    [Fact(DisplayName = "Should load real assembly")]
    public void ShouldLoadTaskFromFile()
    {
        //Arrange
        var extractor = new TaskAssetExtractor(_testAsset);
        
        //Act
        var taskFactories = extractor.Extract().ToArray();
        var found = taskFactories.SingleOrDefault();

        //Assert
        Assert.NotNull(found);
        Assert.Equal("foo", found.Name.Asset);
        Assert.Null(found.Name.LocalName);
        Assert.Equal("WriteToLogTaskStartup", found.StartupType.Name);
    }

    [Fact(DisplayName = "Fail if no tasks")]
    public void ShouldFailIfNoTaskInAssembly()
    {
        //Arrange
        var getAssemblyTypesMock = GetStrategyMock(Array.Empty<TypeDesc>());
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
    }

    [Fact(DisplayName = "Fail if several tasks without names")]
    public void ShouldFailIfSeveralTasksWithoutLocalNames()
    {
        //Arrange
        var getAssemblyTypesMock = GetStrategyMock
        (
            new []
            {
                new TypeDesc
                (
                    Type: typeof(TestTaskStartup), 
                    IsPublic: true,
                    IsImplTaskStartup: true,
                    Name: null
                ),
                new TypeDesc
                (
                    Type: typeof(TestTaskStartup), 
                    IsPublic: true,
                    IsImplTaskStartup: true,
                    Name: null
                ),
            }
        );
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
    }

    
    [Theory(DisplayName = "Should create correct task name")]
    [InlineData("bar")]
    [InlineData(null)]
    public void ShouldCreateCorrectTaskName(string localName)
    {
        //Arrange        
        var getAssemblyTypesMock = GetStrategyMock
        (
            new []
            {
                new TypeDesc
                (
                    Type: typeof(TestTaskStartup), 
                    IsPublic: true,
                    IsImplTaskStartup: true,
                    Name: localName
                ),
            }
        );
        
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act 
        var tasks = extractor.Extract().ToArray();
        var foundTask = tasks.SingleOrDefault();

        //Assert
        Assert.NotNull(foundTask?.Name);
        Assert.Equal("foo", foundTask.Name.Asset);
        Assert.Equal(localName, foundTask.Name.LocalName);
    }

}
