using MyLab.Log;
using MyLab.ProtocolStorage.Client.Models;
using MyLab.Task.RuntimeSdk;
using Newtonsoft.Json;

namespace MyLab.Task.Runtime;

class TaskIterationProtocolEvent : ProtocolEvent
{
    [JsonProperty("workload")]
    [JsonConverter(typeof(TaskEnumJsonConverter))]
    public IterationWorkload Workload { get; set; }

    [JsonProperty("metrics")]
    public IDictionary<string, double>? Metrics { get; set; }
    
    [JsonProperty("duration")]
    [JsonConverter(typeof(TimeSpanToMsJsonConverter))]
    public TimeSpan Duration { get; set; }

    [JsonProperty("error")]
    public ExceptionDto? Error { get; set; }
}
