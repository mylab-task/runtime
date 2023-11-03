using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Task.RuntimeSdk;
using Xunit;

namespace TestTask.UnitTests
{
    public class RequestSenderTaskStartupBehavior
    {
        [Fact]
        public async Task ShouldInitTask()
        {
            //Arrange
            string? lastLogMessage = null;

            var requestSenderMock = new Mock<IRequestSender>();
            requestSenderMock.Setup(l => l.SendAsync (It.IsAny<string>()))
                .Returns<string>(m => 
                {
                    lastLogMessage = m;
                    return Task.CompletedTask;
                });

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { 
                            $"{RequestSenderOptions.SectionName}:{nameof(RequestSenderOptions.Message)}",
                            "foo"
                        }
                    }
                );

            var services = new ServiceCollection()
                .AddSingleton<IRequestSender>(requestSenderMock.Object);

            var app = new RequestSenderTaskStartup();

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
