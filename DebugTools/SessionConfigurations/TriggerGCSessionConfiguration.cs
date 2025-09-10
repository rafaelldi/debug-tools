using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using Monitor.Common;

namespace Monitor.SessionConfigurations;

internal sealed class TriggerGCSessionConfiguration(int processId) : BaseSessionConfiguration(processId)
{
    internal override EventPipeProvider Provider { get; } = new(
        ProviderNames.MicrosoftWindowsDotNetRuntime,
        EventLevel.Informational,
        (long)ClrTraceEventParser.Keywords.GCHeapCollect,
        new Dictionary<string, string>()
    );
}