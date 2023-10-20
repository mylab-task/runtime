using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace TestTask.UnitTests;

public class WriteToLogTaskLogicBehavior
{
    [Fact]
    public async Task ShouldWriteLog()
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

        var options = new WriteToLogOptions
        {
            Message = "foo"
        };

        var task = new WriteToLogTaskLogic(loggerMock.Object, new OptionsWrapper<WriteToLogOptions>(options));

        //Act
        await task.PerformAsync(new MyLab.Task.RuntimeSdk.TaskIterationContext("", default), default);

        //Assert
        Assert.Equal("foo", lastLogMessage);
    }
}