using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;

namespace IntegrationTests;

public partial class TaskAssetExtractorBehavior
{
    TaskAssetSource _testAsset;

    public TaskAssetExtractorBehavior()
    {
        var testAssemblyLoader = new FileAssemblyLoader(TestStuff.GetTestAssemblyPath());
        _testAsset = new TaskAssetSource("foo", testAssemblyLoader);
    }

    TaskAssetExtractor.IStrategy GetStrategyMock(
        IEnumerable<TaskAssetExtractor.TypeDesc> types)
    {
        var asselbyTypesProviderMock = new Mock<TaskAssetExtractor.IStrategy>();
        asselbyTypesProviderMock.Setup(p => p.GetAssemblyTypes(It.IsAny<Assembly>()))
            .Returns<Assembly>(a => types);

        return asselbyTypesProviderMock.Object;
    }

    class TestTaskStartup : ITaskStartup
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
