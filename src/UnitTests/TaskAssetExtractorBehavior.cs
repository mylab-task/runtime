using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;

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
        Assert.Equal("WriteToLogTaskLogic", found.LogicType.Name);
        Assert.Equal("WriteToLogTaskStartup", found.StartupType.Name);
    }

    [Fact(DisplayName = "Fail if no tasks")]
    public void ShouldFailIfNoTaskInAssembly()
    {
        //Arrange
        var getAssemblyTypesMock = GetAsmProviderMock
        (
            Enumerable.Empty<TaskAssetExtractor.TaskLogicTypeDesc>(),
            null
        );

        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
    }

    [Fact(DisplayName = "Fail if several tasks without names")]
    public void ShouldFailIfSeveralTasksWithoutLocalNames()
    {
        //Arrange
        var getAssemblyTypesMock = GetAsmProviderMock
        (
            Enumerable.Empty<TaskAssetExtractor.TaskLogicTypeDesc>(),
            null
        );
        
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
    }

    [Fact(DisplayName = "Fail if task without startup and no default one")]
    public void ShouldFailIfNoTaskStartupAndDefaultStartup()
    {
        //Arrange
        var getAssemblyTypesMock = GetAsmProviderMock
        (
            new []{ new TaskAssetExtractor.TaskLogicTypeDesc(typeof(TestTypeLogic), null) },
            null
        );
        
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
    }

    [Fact(DisplayName = "Load with deafult startup")]
    public void ShouldLoadWithDefaultStartup()
    {
        //Arrange
        var getAssemblyTypesMock = GetAsmProviderMock
        (
            new []{ new TaskAssetExtractor.TaskLogicTypeDesc(typeof(TestTypeLogic), null) },
            typeof(TestTaskStartup)
        );
        
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act 
        var tasks = extractor.Extract().ToArray();
        var foundTask = tasks.SingleOrDefault();

        //Assert
        Assert.NotNull(foundTask);
        Assert.Equal(typeof(TestTypeLogic), foundTask.LogicType);
        Assert.Equal(typeof(TestTaskStartup), foundTask.StartupType);
    }

    [Fact(DisplayName = "Load with explicite startup")]
    public void ShouldLoadWithExpliciteStartup()
    {
        //Arrange
        var getAssemblyTypesMock = GetAsmProviderMock
        (
            new []
            { 
                new TaskAssetExtractor.TaskLogicTypeDesc
                (
                    typeof(TestTypeLogic), 
                    new TaskAttribute
                    {
                        Startup = typeof(TestTaskStartup)
                    }
                ) 
            },
            null
        );
        
        var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
        //Act 
        var tasks = extractor.Extract().ToArray();
        var foundTask = tasks.SingleOrDefault();

        //Assert
        Assert.NotNull(foundTask);
        Assert.Equal(typeof(TestTypeLogic), foundTask.LogicType);
        Assert.Equal(typeof(TestTaskStartup), foundTask.StartupType);
    }

    [Theory(DisplayName = "Should create correct task name")]
    [InlineData("bar")]
    [InlineData(null)]
    public void ShouldCreateCorrectTaskName(string localName)
    {
        //Arrange        
        var getAssemblyTypesMock = GetAsmProviderMock
        (
            new []
            { 
                new TaskAssetExtractor.TaskLogicTypeDesc
                (
                    typeof(TestTypeLogic), 
                    new TaskAttribute
                    {
                        Startup = typeof(TestTaskStartup),
                        Name = localName
                    }
                ) 
            },
            null
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
