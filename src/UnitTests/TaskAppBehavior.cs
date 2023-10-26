using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Moq;
using MyLab.Task.Runtime;
using MyLab.Task.RuntimeSdk;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace UnitTests;

public class TaskAppBehavior
{
    [Fact(DisplayName = "Should perform logic")]
    public async Task ShouldPerformTaskLogic()
    {
        //Arrange
        var taskLogicMock = new Mock<ITaskLogic>(); 

        var serviceProvider = new ServiceCollection()
            .AddScoped(sp => taskLogicMock.Object)
            .BuildServiceProvider();
        var taskApp = new TaskApp(new TaskQualifiedName("foo", "bar"), serviceProvider);

        //Act
        await taskApp.PerformIterationAsync(CancellationToken.None);

        //Assert
        taskLogicMock.Verify(l => l.PerformAsync(It.IsAny<TaskIterationContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should create scoped")]
    public async Task ShouldCreateScopedForeachPerform()
    {
        //Arrange
        int taskCreationCounter = 0;
        var taskLogicMock = new Mock<ITaskLogic>(); 

        var serviceProvider = new ServiceCollection()
            .AddScoped(sp => 
            {
                taskCreationCounter++;
                return taskLogicMock.Object;
            })
            .BuildServiceProvider();

        var taskApp = new TaskApp(new TaskQualifiedName("foo", "bar"), serviceProvider);

        //Act
        await taskApp.PerformIterationAsync(CancellationToken.None);
        await taskApp.PerformIterationAsync(CancellationToken.None);

        //Assert
        Assert.Equal(2, taskCreationCounter);
    }

    [Fact(DisplayName = "Should use trace id")]
    public async Task ShouldSetIterationIdFromTraceId()
    {
        //Arrange
        string? catchedInterationId = null;
        var taskLogicMock = new Mock<ITaskLogic>();
        taskLogicMock.Setup(l => l.PerformAsync(It.IsAny<TaskIterationContext>(), It.IsAny<CancellationToken>()))
            .Callback<TaskIterationContext,CancellationToken>((ctx, ct) => catchedInterationId = ctx.TraceId); 

        var serviceProvider = new ServiceCollection()
            .AddScoped(sp => taskLogicMock.Object)
            .BuildServiceProvider();

        var taskApp = new TaskApp(new TaskQualifiedName("foo", "bar"), serviceProvider);

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("TestActivitySource")
            .Build();

        var activitySource = new ActivitySource("TestActivitySource");

        string? actialActivityId;

        //Act
        using(var activity = activitySource.StartActivity("test-activity"))
        {
            actialActivityId = activity?.TraceId.ToHexString();
            await taskApp.PerformIterationAsync(CancellationToken.None);
        }

        //Assert
        Assert.NotNull(actialActivityId);
        Assert.Equal(actialActivityId, catchedInterationId);
    }

    [Fact(DisplayName = "Should add task log scopes")]
    public async Task ShouldAddTaskLogSopes()
    {
        //Arrange
        var taskLogicMock = new Mock<ITaskLogic>();
        
        var serviceProvider = new ServiceCollection()
            .AddLogging(l => l
                .AddConsole()
                .AddConsoleFormatter<TestFormatter, ConsoleFormatterOptions>()
            )
            .Configure<ConsoleLoggerOptions>(o => o.FormatterName = TestFormatter.StaticName)
            .AddScoped(sp => taskLogicMock.Object)
            .BuildServiceProvider();
        
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("test");
        
        taskLogicMock.Setup(l => l.PerformAsync(It.IsAny<TaskIterationContext>(), It.IsAny<CancellationToken>()))
            .Callback<TaskIterationContext,CancellationToken>((ctx, ct) => 
            {
                logger.LogError("error");
            }); 

        var taskApp = new TaskApp(new TaskQualifiedName("foo", "bar"), serviceProvider);

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("TestActivitySource")
            .Build();

        var activitySource = new ActivitySource("TestActivitySource");

        TestFormatter.Catched.Clear();

        //Act
        using(var activity = activitySource.StartActivity("test-activity"))
        {
            await taskApp.PerformIterationAsync(CancellationToken.None);
        }

        //Assert
        Assert.Contains(TestFormatter.Catched, s => s.Key == LogScopes.TaskNameFact);
        Assert.Contains(TestFormatter.Catched, s => s.Key == MyLab.Log.PredefinedLabels.TraceId);
    }

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
        
        //Act
        TaskApp taskApp = TaskApp.Create(new TaskQualifiedName("baz", null), startupMock.Object, baseConfig);
        
        //Assert
        Assert.Equal("baz", taskApp.Name.Asset);

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

    class TestFormatter : ConsoleFormatter
    {
        public static readonly string StaticName = "test";

        public static readonly List<KeyValuePair<string, object>> Catched 
            = new List<KeyValuePair<string, object>>();

        public TestFormatter() : base(StaticName)
        {
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            scopeProvider.ForEachScope((scope, state) => 
            {
                if(scope is IEnumerable<KeyValuePair<string, object>> d)
                {
                    state.AddRange(d);
                }
            }, Catched);
        }
    }
}