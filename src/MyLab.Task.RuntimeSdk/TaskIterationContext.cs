using System;

namespace MyLab.Task.RuntimeSdk
{
    /// <summary>
    /// Provides access to task logic iteration context  
    /// </summary>
    public class TaskIterationContext
    {
        /// <summary>
        /// Trace identifier
        /// </summary>
        public string? TraceId { get; }

        /// <summary>
        /// Date and time of iteration start
        /// </summary>
        public DateTime StartAt { get; }

        /// <summary>
        /// Iteration report. 'null' by default.
        /// </summary>
        public IterationReport? Report { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TaskIterationContext"/>
        /// </summary>
        public TaskIterationContext(string? traceId, DateTime startAt)
        {
            TraceId = traceId;
            StartAt = startAt;
        }
    }
}