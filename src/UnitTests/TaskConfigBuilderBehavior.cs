using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MyLab.Task.Runtime;
using Xunit;

namespace UnitTests
{
    public class TaskConfigBuilderBehavior
    {
        [Fact(DisplayName = "Should add app log options")]
        public void ShouldAddAppLogOptions()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { $"Logging:LogLevel:Default", "Information" }
                    }
                )
                .Build();
    
            var builder = TaskConfigBuilder.FromConfig(config, "Runtime");

            //Act        
            var taskConfig = builder.Build("mytask");
        
            //Assert
            Assert.Equal("Information", taskConfig["Logging:LogLevel:Default"]);
        }

        [Fact(DisplayName = "Should add base task log options")]
        public void ShouldAddBaseTaskLogOptions()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { $"Logging:LogLevel:Default", "Information" },
                        { $"Runtime:{nameof(RuntimeOptions.BaseTaskConfig)}:Logging:LogLevel:Default", "Error" }
                    }
                )
                .Build();
    
            var builder = TaskConfigBuilder.FromConfig(config, "Runtime");

            //Act        
            var taskConfig = builder.Build("mytask");
        
            //Assert
            Assert.Equal("Error", taskConfig["Logging:LogLevel:Default"]);
        }

        [Fact(DisplayName = "Should add task log options")]
        public void ShouldAddTaskLogOptions()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { $"Logging:LogLevel:Default", "Information" },
                        { $"Runtime:{nameof(RuntimeOptions.BaseTaskConfig)}:Logging:LogLevel:Default", "Warning" },
                        { $"Runtime:{nameof(RuntimeOptions.Tasks)}:mytask:{nameof(TaskOptions.Config)}:Logging:LogLevel:Default", "Error" }
                    }
                )
                .Build();
    
            var builder = TaskConfigBuilder.FromConfig(config, "Runtime");

            //Act        
            var taskConfig = builder.Build("mytask");
        
            //Assert
            Assert.Equal("Error", taskConfig["Logging:LogLevel:Default"]);
        }

        [Fact(DisplayName = "Should add task base options")]
        public void ShouldAddTaskBaseOptions()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { $"Runtime:{nameof(RuntimeOptions.BaseTaskConfig)}:BaseParam", "foo" }
                    }
                )
                .Build();
    
            var builder = TaskConfigBuilder.FromConfig(config, "Runtime");

            //Act        
            var taskConfig = builder.Build("mytask");
        
            //Assert
            Assert.Equal("foo", taskConfig["BaseParam"]);
        }

        [Fact(DisplayName = "Should add task options")]
        public void ShouldAddTaskOptions()
        {
            //Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { $"Runtime:{nameof(RuntimeOptions.BaseTaskConfig)}:Param", "foo" },
                        { $"Runtime:{nameof(RuntimeOptions.Tasks)}:mytask:{nameof(TaskOptions.Config)}:Param", "bar" }
                    }
                )
                .Build();
    
            var builder = TaskConfigBuilder.FromConfig(config, "Runtime");

            //Act        
            var taskConfig = builder.Build("mytask");
        
            //Assert
            Assert.Equal("bar", taskConfig["Param"]);
        }
    }
}