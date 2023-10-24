using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;

namespace IntegrationTests;

public class TaskFactoryBehavior
{
    [Fact]
    public void ShouldCreateStartup()
    {
        //Arrange
        var factory = new TaskStartupFactory(new TaskQualifiedName("foo", null), typeof(TestStartup));

        //Act
        var startup = factory.Create();

        //Assert
        Assert.NotNull(startup);
        Assert.IsType<TestStartup>(startup);
    }

    class TestStartup : ITaskStartup
    {
        public void AddConfiguration(IConfigurationBuilder configBuilder)
        {
            throw new NotImplementedException();
        }

        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}