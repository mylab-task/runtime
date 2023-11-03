using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Task.Runtime;
using TestTask;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class RuntimeBehavior
    {
        private ITestOutputHelper _output;

        public RuntimeBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "Should work idle")]
        public async Task ShouldWorkIdle()
        {
            //Arrange
            var emptyTaskAssetProvider = Mock.Of<ITaskAssetProvider>(p => p.Provide() == Enumerable.Empty<TaskAssetSource>());

            bool thereWasErrorLog = false;
            var testLoggerMock = new Mock<ILogger>();
        
            testLoggerMock.Setup(l => l.Log<string>
                (
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(), 
                    It.IsAny<string>(), 
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<string, Exception?, string>>()
                ))
                .Callback<LogLevel, EventId, string, Exception?, Func<string, Exception?, string>>
                (
                    (l, _, _, _, _) => 
                    {
                        if(l > LogLevel.Error) thereWasErrorLog = true;
                    }
                );
        
            var testLoggerProvider = new Mock<ILoggerProvider>(); 
            testLoggerProvider.Setup(p => p.CreateLogger(It.IsAny<string>()))
                .Returns<string>(_ => testLoggerMock.Object);

            var config = new ConfigurationBuilder()
                .Build();

            var services = new ServiceCollection()
                .AddTaskRuntime()
                .AddSingleton<ITaskAssetProvider>(emptyTaskAssetProvider)
                .AddSingleton<IConfiguration>(config)
                .AddLogging
                (
                    lb => lb
                        .AddFilter(_ => true)
                        .AddXUnit(_output)
                        .AddProvider(testLoggerProvider.Object)
                )
                .BuildServiceProvider();

            var hostedServices = services.GetServices<IHostedService>();
        
            var cancelSource = new CancellationTokenSource();

            //Act
            var tasks = hostedServices
                .Select(s => s.StartAsync(cancelSource.Token))
                .ToArray();
        
            await Task.Delay(1000);

            cancelSource.Cancel();
            Task.WaitAll(tasks);

            //Assert
            testLoggerProvider.Verify(p => p.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
            Assert.False(thereWasErrorLog);
        }

        [Theory(DisplayName = "Should perform task")]
        [InlineData(1, 2)]
        [InlineData(0, 0)]
        public async Task ShouldPerformTaskPeriodically(int timeSpanSeconds, int performCount)
        {
            //Arrange
            var testTaskAssembly = typeof(IRequestSender).Assembly;

            var testAssetAssemblyLoaderMock = new Mock<IAssemblyLoader>(); 
            testAssetAssemblyLoaderMock.Setup(l => l.Load(It.IsAny<AssemblyLoadContext>()))
                .Returns<AssemblyLoadContext>(ctx => testTaskAssembly);

            var testTaskAssert = new TaskAssetSource("test", testAssetAssemblyLoaderMock.Object);

            var testTaskAssetProvider = Mock.Of<ITaskAssetProvider>(p => p.Provide() == new TaskAssetSource[]{testTaskAssert});

            var requestSenderMock = new Mock<IRequestSender>();

            requestSenderMock.Setup(s => s.SendAsync(It.IsAny<string>()))
                .Returns<string>(msg => 
                {
                    _output.WriteLine(">>> REQUEST SENT!\r\n");
                    return Task.CompletedTask;
                });
        
            var testTaskServicesPostProcessingMock = new Mock<ITaskServicesPostProcessing>();
            testTaskServicesPostProcessingMock
                .Setup(spp => spp.PostProcess(It.IsAny<IServiceCollection>()))
                .Returns<IServiceCollection>(s => s.AddSingleton<IRequestSender>(requestSenderMock.Object));

            var config = new ConfigurationBuilder()
                .Build();

            var taskConfigRoot = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { 
                            $"TaskConfig:{RequestSenderOptions.SectionName}:{nameof(RequestSenderOptions.Message)}",
                            "foo"
                        }
                    }                
                )
                .Build();

            var taskConfigSection = new ConfigurationSection(taskConfigRoot, "TaskConfig");

            var services = new ServiceCollection()
                .AddTaskRuntime()
                .ConfigureTaskRuntime(o =>
                {
                    o.Tasks = new Dictionary<string, TaskOptions>
                    {
                        { 
                            "test" , 
                            new TaskOptions
                            {
                                Period = TimeSpan.FromSeconds(timeSpanSeconds),
                                Config = taskConfigSection
                            }
                        }
                    };
                })
                .AddSingleton<ITaskAssetProvider>(testTaskAssetProvider)
                .AddSingleton<IConfiguration>(config)
                .AddSingleton<ITaskServicesPostProcessing>(testTaskServicesPostProcessingMock.Object)
                .AddLogging
                (
                    lb => lb
                        .AddFilter(_ => true)
                        .AddXUnit(_output)
                )
                .BuildServiceProvider();

            var hostedServices = services.GetServices<IHostedService>();
        
            var cancelSource = new CancellationTokenSource();

            //Act
            var tasks = hostedServices
                .Select(s => s.StartAsync(cancelSource.Token))
                .ToArray();
        
            await Task.Delay(TimeSpan.FromSeconds(2.5));

            cancelSource.Cancel();
            Task.WaitAll(tasks);

            //Assert
            requestSenderMock.Verify(sender => sender.SendAsync(It.Is<string>(msg => msg == "foo")), Times.Exactly(performCount));
        }
    }
}