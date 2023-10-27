namespace MyLab.Task.Runtime;

public class RuntimeOptions
{
    public string AssetsPath { get; set; } = "/etc/task-runtime/assets";

    public Dictionary<string, TaskOptions>? Tasks { get; set; }

    public TimeSpan DefaultPeriod { get; set; } = TimeSpan.FromMinutes(1);

    public IConfigurationSection? BaseTaskConfig { get; set; }

}
