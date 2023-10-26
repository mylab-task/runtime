namespace MyLab.Task.Runtime;

using MyLab.Log.Dsl;
using Task = System.Threading.Tasks.Task;

class Scheduler
{
    private TimeSpan _basePeriod;
    private List<RegisteredTask> _tasks = new List<RegisteredTask>();

    public IDslLogger? Logger{get;set;}

    public Scheduler(TimeSpan basePeriod)
    {
        _basePeriod = basePeriod;
    }

    public void RegisterTask(ITaskPerformer taskPerformer, TimeSpan period)
    {
        _tasks.Add(new RegisteredTask(taskPerformer, period));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if(_basePeriod == default)
            return;

        while(!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_basePeriod);

            foreach (var t in _tasks)
            {
                ProcessRegisteredTask(t,cancellationToken);
            }
        }
    }

    private void ProcessRegisteredTask(RegisteredTask t, CancellationToken cancellationToken)
    {
        if(IsActive(t)) return;

        try
        {
            if(DateTime.Now - t.StartedAt > t.Period)
            {
                t.Task = t.Performer.PerformIterationAsync(cancellationToken);
                t.Task.Start();
                t.StartedAt = DateTime.Now;
            }
        }
        catch(Exception unhandledTaskEx)
        {
            Logger?
                .Error("Task performing error", unhandledTaskEx)
                .AndLabel(LogScopes.TaskName, t.Performer.TaskName.ToString())
                .Write();
        }
    }

    private bool IsActive(RegisteredTask t)
        =>  t.Task != null && 
            t.Task.Status != TaskStatus.Canceled && 
            t.Task.Status != TaskStatus.Faulted &&
            t.Task.Status != TaskStatus.RanToCompletion;

    class RegisteredTask
    {
        public Task? Task{get;set;}

        public DateTime StartedAt {get;set;}

        public ITaskPerformer Performer{get;}

        public TimeSpan Period {get;}

        public RegisteredTask(ITaskPerformer taskPerformer, TimeSpan period)
        {
            Performer = taskPerformer;
            Period = period;
        }
    }
}
