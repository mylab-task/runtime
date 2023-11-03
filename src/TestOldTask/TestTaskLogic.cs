using System.Threading;
using System.Threading.Tasks;
using MyLab.Task.RuntimeSdk;

namespace TestOldTask
{
    class TestTaskLogic : ITaskLogic
    {
        public ValueTask PerformAsync(TaskIterationContext iterationContext, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }
}
