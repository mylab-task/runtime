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

public class TaskPerformerBehavior
{
    [Fact(DisplayName = "Should perform logic")]
    public async Task ShouldPerformTaskLogic()
    {
        //Arrange
        var taskLogicMock = new Mock<ITaskLogic>(); 

        var serviceProvider = new ServiceCollection()
            .AddScoped(sp => taskLogicMock.Object)
            .BuildServiceProvider();
        var taskApp = new TaskPerformer(new TaskQualifiedName("foo", "bar"), serviceProvider);

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

        var taskApp = new TaskPerformer(new TaskQualifiedName("foo", "bar"), serviceProvider);

        //Act
        await taskApp.PerformIterationAsync(CancellationToken.None);
        await taskApp.PerformIterationAsync(CancellationToken.None);

        //Assert
        Assert.Equal(2, taskCreationCounter);
    }

    [Fact(DisplayName = "Should set trace activity properties")]
    public async Task ShouldSetIterationIdFromTraceId()
    {
        //Arrange
        string? catchedInterationId = null;
        string? catchedTraceId = null;
        string? catchedInterationActivityName = null;

        var taskLogicMock = new Mock<ITaskLogic>();
        taskLogicMock.Setup(l => l.PerformAsync(It.IsAny<TaskIterationContext>(), It.IsAny<CancellationToken>()))
            .Callback<TaskIterationContext,CancellationToken>
            (
                (ctx, ct) => 
                {
                    catchedInterationId = ctx.TraceId;
                    catchedTraceId = Activity.Current?.TraceId.ToString();
                    catchedInterationActivityName = Activity.Current?.OperationName;
                }
            ); 

        var serviceProvider = new ServiceCollection()
            .AddScoped(sp => taskLogicMock.Object)
            .BuildServiceProvider();

        var activitySource = new ActivitySource("TestActivitySource");

        var taskApp = new TaskPerformer(new TaskQualifiedName("foo", "bar"), serviceProvider)
        {
            ActivitySource = activitySource
        };

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(activitySource.Name)
            .Build();

        //Act
        await taskApp.PerformIterationAsync(CancellationToken.None);

        //Assert
        Assert.NotNull(catchedInterationId);
        Assert.Equal(TaskPerformer.TaskPerformingActivityName, catchedInterationActivityName);
        Assert.Equal(catchedInterationId, catchedTraceId);
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

        var activitySource = new ActivitySource("TestActivitySource");
        var taskApp = new TaskPerformer(new TaskQualifiedName("foo", "bar"), serviceProvider)
        {
            ActivitySource = activitySource
        };

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(activitySource.Name)
            .Build();
            
        TestFormatter.Catched.Clear();

        //Act
        using(var activity = activitySource.StartActivity("test-activity"))
        {
            await taskApp.PerformIterationAsync(CancellationToken.None);
        }

        //Assert
        Assert.Contains(TestFormatter.Catched, s => s.Key == LogScopes.TaskName);
        Assert.Contains(TestFormatter.Catched, s => s.Key == MyLab.Log.PredefinedLabels.TraceId);
    }

    class TestFormatter : ConsoleFormatter
    {
        public static readonly string StaticName = "test";

        public static readonly List<KeyValuePair<string, string>> Catched 
            = new List<KeyValuePair<string, string>>();

        public TestFormatter() : base(StaticName)
        {
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            scopeProvider.ForEachScope((scope, state) => 
            {
                if(scope is IEnumerable<KeyValuePair<string, string>> d)
                {
                    state.AddRange(d);
                }
            }, Catched);
        }
    }
}