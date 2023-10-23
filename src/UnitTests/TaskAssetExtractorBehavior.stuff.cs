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

    TaskAssetExtractor.IAssemblyTypesProvider GetAsmProviderMock(
        IEnumerable<TaskAssetExtractor.TaskLogicTypeDesc> logicTypes,
        Type? defaultStartupType)
    {
        var asselbyTypesProviderMock = new Mock<TaskAssetExtractor.IAssemblyTypesProvider>();
        asselbyTypesProviderMock.Setup(p => p.GetTaskLogicTypes(It.IsAny<Assembly>()))
            .Returns<Assembly>(a => logicTypes);
        asselbyTypesProviderMock.Setup(p => p.GetDefaultStartupType(It.IsAny<Assembly>()))
            .Returns<Assembly>(a => defaultStartupType);

        return asselbyTypesProviderMock.Object;
    }

    class TestTypeLogic : ITaskLogic
    {
        public ValueTask PerformAsync(TaskIterationContext iterationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
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
