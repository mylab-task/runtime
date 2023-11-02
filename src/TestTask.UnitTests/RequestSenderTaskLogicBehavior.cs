using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace TestTask.UnitTests;

public class RequestSenderTaskLogicBehavior
{
    [Fact]
    public async Task ShouldWriteLog()
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

        var options = new RequestSenderOptions
        {
            Message = "foo"
        };

        var task = new RequestSenderTaskLogic(requestSenderMock.Object, new OptionsWrapper<RequestSenderOptions>(options));

        //Act
        await task.PerformAsync(new MyLab.Task.RuntimeSdk.TaskIterationContext("", default), default);

        //Assert
        Assert.Equal("foo", lastLogMessage);
    }
}