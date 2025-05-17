using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using MonitorAgent.Common;

namespace MonitorAgent.SessionConfigurations;

internal sealed class TriggerGCSessionConfiguration(int processId) : AbstractSessionConfiguration(processId)
{
    internal override EventPipeProvider Provider { get; } = new(
        ProviderNames.MicrosoftWindowsDotNetRuntime,
        EventLevel.Informational,
        (long)ClrTraceEventParser.Keywords.GCHeapCollect,
        new Dictionary<string, string>()
    );
}