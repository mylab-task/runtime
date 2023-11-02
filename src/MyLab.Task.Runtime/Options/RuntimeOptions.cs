﻿namespace MyLab.Task.Runtime;

public class RuntimeOptions
{
    public string AssetsPath { get; set; } = "/etc/task-runtime/assets";

    public Dictionary<string, TaskOptions>? Tasks { get; set; }

    public IConfigurationSection? BaseTaskConfig { get; set; }

    public string ProtocolId { get; set; } = "tasks";

}
