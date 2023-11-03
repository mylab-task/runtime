using System;
using Microsoft.Extensions.Configuration;

namespace MyLab.Task.Runtime
{
    public class TaskOptions
    {
        public TimeSpan? Period { get; set; }

        public IConfigurationSection? Config { get; set; }
    }
}
