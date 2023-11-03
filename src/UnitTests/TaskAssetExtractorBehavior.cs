using System;
using System.Linq;
using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;
using Xunit;
using TypeDesc = MyLab.Task.Runtime.TaskAssetExtractor.TypeDesc;

namespace UnitTests
{
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
            Assert.Equal("RequestSenderTaskStartup", found.StartupType.Name);
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
                        HasGenericParams: false,
                        HasPubDefCtor: true,
                        Name: null
                    ),
                    new TypeDesc
                    (
                        Type: typeof(TestTaskStartup), 
                        IsPublic: true,
                        IsImplTaskStartup: true,
                        HasGenericParams: false,
                        HasPubDefCtor: true,
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
                        HasGenericParams: false,
                        HasPubDefCtor: true,
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

        [Fact(DisplayName = "Ignore non-public startups")]
        public void ShouldIgnoreNonPublicStartups()
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
                        HasGenericParams: false,
                        HasPubDefCtor: true,
                        Name: "foo"
                    ),
                    new TypeDesc
                    (
                        Type: typeof(TestTaskStartup), 
                        IsPublic: false,
                        IsImplTaskStartup: true,
                        HasGenericParams: false,
                        HasPubDefCtor: true,
                        Name: "bar"
                    ),
                }
            );
        
            var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
            //Act 
            var tasks = extractor.Extract().ToArray();
            var foundTask = tasks.SingleOrDefault();

            //Assert
            Assert.Equal("foo", foundTask?.Name.LocalName);
        }

        [Fact(DisplayName = "Fail if generic startup")]
        public void ShoudFailIfGenericStartup()
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
                        HasGenericParams: true,
                        HasPubDefCtor: true,
                        Name: null
                    ),
                }
            );
            var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
        }

        [Fact(DisplayName = "Fail if has no public default constructor")]
        public void ShoudFailIfHasNoPubDefCtor()
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
                        HasGenericParams: false,
                        HasPubDefCtor: false,
                        Name: null
                    ),
                }
            );
            var extractor = new TaskAssetExtractor(_testAsset, getAssemblyTypesMock);
        
            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => extractor.Extract().ToArray());
        }

    }
}
