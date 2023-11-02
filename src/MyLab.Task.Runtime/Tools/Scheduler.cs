namespace MyLab.Task.Runtime;

using MyLab.Log.Dsl;
using MyLab.Log.Scopes;
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
        Logger?
            .Action("The scheduler starts")
            .Write();

        if(_basePeriod == default)
        {
            Logger?
                .Warning("The base scheduler period is default. Run will be skipped!")
                .AndFactIs("base-period", _basePeriod)
                .Write();

            return;
        }

        while(!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_basePeriod);

            Logger?
                .Debug("Scheduler tick")
                .Write();

            foreach (var t in _tasks)
            {
                using(Logger?.BeginScope(new LabelLogScope(LogLabels.TaskName, t.Performer.TaskName.ToString())))
                {
                    ProcessRegisteredTask(t,cancellationToken);
                }
            }
        }

        Logger?
            .Action("The scheduler stops")
            .Write();
    }

    private void ProcessRegisteredTask(RegisteredTask t, CancellationToken cancellationToken)
    {
               
        try
        {
            var passedTime = DateTime.Now - t.StartedAt;

            if(passedTime > t.Period)
            {
                Logger?
                    .Action("It's time to perform the task")
                    .AndFactIs("passed", passedTime)
                    .Write();

                if(IsActive(t))
                {
                    Logger?
                        .Action("A task performing will be skipped due to task already performing")
                        .Write();
                    return;
                }

                Logger?
                    .Action("Task performing started")
                    .Write();

                t.Task = CallPerformerAsync(t.Performer, cancellationToken);

                if(t.Task.Status == TaskStatus.Created)
                {
                    t.Task.Start();
                }

                t.StartedAt = DateTime.Now;

                Logger?
                    .Action("A task performing has been completed")
                    .Write();
            }
        }
        catch(Exception unhandledTaskEx)
        {
            Logger?
                .Error("Task performing starting error", unhandledTaskEx)
                .Write();
        }
    }

    async Task CallPerformerAsync(ITaskPerformer performer, CancellationToken cancellationToken)
    {
        try
        {
            await performer.PerformIterationAsync(cancellationToken);
            Logger?
                .Action("The task was completed")
                .Write();
        }
        catch(Exception e)
        {
            Logger?
                .Error("The performing error", e)
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
