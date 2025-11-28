using System.Collections.Generic;

namespace MyLab.Task.RuntimeSdk
{
    /// <summary>
    /// Task iteration report
    /// </summary>
    public class IterationReport
    {
        /// <summary>
        /// The identifier which correlate with task iteration
        /// </summary>
        public string? IterationId { get; set; }

        /// <summary>
        /// Gets or sets iteration workload
        /// </summary>
        public IterationWorkload Workload { get; set; }
        
        /// <summary>
        /// Gets or sets context subject identifier 
        /// </summary>
        public string? SubjectId { get; set; }

        /// <summary>
        /// Gets or sets business-level named numeric metrics
        /// </summary>
        public IDictionary<string, double>? Metrics { get; set; }
    }
}