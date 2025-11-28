using System;

namespace MyLab.Task.RuntimeSdk
{
    /// <summary>
    /// Defines type meta-properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TaskNameAttribute : Attribute
    {
        /// <summary>
        /// Task identical name
        /// </summary>
        public string Name { get; }

        public TaskNameAttribute(string name)
        {
            Name = name;
        }
    }
}