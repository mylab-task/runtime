using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Task.RuntimeSdk;

namespace TestTask.UnitTests
{
    public class WriteToLogTaskStartupBehavior
    {
        [Fact]
        public async Task ShouldInitTask()
        {
            //Arrange
            string? lastLogMessage = null;

            var loggerMock = new Mock<ILogger>();
            loggerMock
                .Setup(l => l.Log
                    (
                       It.IsAny<LogLevel>(),
                       It.IsAny<EventId>(),
                       It.IsAny<string>(),
                       It.IsAny<Exception?>(),
                       It.IsAny<Func<string, Exception?, string>>()
                    ))
                .Callback<LogLevel, EventId, string, Exception?, Func<string, Exception?, string>>
                    (
                        (_, _, msg, _, _) => lastLogMessage = msg
                    );

            var configBuilder = new ConfigurationBuilder();
            var services = new ServiceCollection()
                .AddSingleton<ILogger>(loggerMock.Object)
                .Configure<WriteToLogOptions>(o => o.Message = "foo");

            var app = new WriteToLogTaskStartup();

            //Act
            app.AddConfiguration(configBuilder);
            var config = configBuilder.Build();
            
            app.AddServices(services, config);
            var serviceProvider = services.BuildServiceProvider();

            var task = serviceProvider.GetRequiredService<ITaskLogic>();
            await task.PerformAsync(new TaskIterationContext("", default), default);

            //Arrange
            Assert.Equal("foo", lastLogMessage);
        }
    }
}
