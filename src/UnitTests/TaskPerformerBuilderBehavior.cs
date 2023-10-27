using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;

namespace UnitTests;

public class TaskPerformerBuilderBehavior
{
    [Fact(DisplayName = "Shoud create and init TaskApp")]
    public void ShouldCrateAndInitStartup()
    {
        //Arrange
        var baseConfig = new ConfigurationBuilder()
            .AddInMemoryCollection
            (
                new [] { new KeyValuePair<string, string>("foo", "bar") }
            ).Build();
        var startupMock = new Mock<ITaskStartup>();

        bool servicesHasBeenProcessed = false;

        var builder = new TaskPerformerBuilder(new TaskQualifiedName("baz", null), startupMock.Object)
        {
            BaseConfig = baseConfig,
            PostServiceProc = _ => servicesHasBeenProcessed = true
        }; 
        
        //Act
        ITaskPerformer taskApp = builder.Build();
        
        //Assert
        Assert.Equal("baz", taskApp.TaskName.Asset);
        Assert.True(servicesHasBeenProcessed);
        startupMock.Verify
        (
            s => s.AddConfiguration
            (
                It.Is<IConfigurationBuilder>(c => c.Build()["foo"] == "bar")
            )
        );
        startupMock.Verify
        (
            s => s.AddServices
            (
                It.Is<IServiceCollection>(s => s != null), 
                It.Is<IConfiguration>(c => c["foo"] == "bar")
            )
        );
    }
}