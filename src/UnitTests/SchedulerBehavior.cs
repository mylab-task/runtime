using Moq;
using MyLab.Task.Runtime;

namespace UnitTests;

public class SchedulerBehavior
{
    [Fact(DisplayName = "Should perform periodically")]
    public async Task ShouldPerformTaskPeriodically()
    {
        //Arrange
        var taskPerformerMock = new Mock<ITaskPerformer>();

        var s = new Scheduler(TimeSpan.FromSeconds(1));
        
        s.RegisterTask(taskPerformerMock.Object, TimeSpan.FromSeconds(1));

        var cSource = new CancellationTokenSource();

        //Act
        var t = s.RunAsync(cSource.Token);

        await Task.Delay(2000);
        cSource.Cancel();
        t.Wait();

        //Assert
        taskPerformerMock.Verify(p => p.PerformIterationAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Should not perform if already performing")]
    public async Task ShouldNotPerformIfAlreadyPerforming()
    {
        //Arrange
        var taskPerformerMock = new Mock<ITaskPerformer>();
        taskPerformerMock.Setup(t => t.PerformIterationAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct => Task.Delay(2000));

        var s = new Scheduler(TimeSpan.FromSeconds(1));
        
        s.RegisterTask(taskPerformerMock.Object, TimeSpan.FromSeconds(1));

        var cSource = new CancellationTokenSource();

        //Act
        var t = s.RunAsync(cSource.Token);

        await Task.Delay(2000);
        cSource.Cancel();

        //Assert
        taskPerformerMock.Verify(p => p.PerformIterationAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}