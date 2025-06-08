using Microsoft.Diagnostics.NETCore.Client;
using MonitorAgent;

namespace Monitor.MemoryDumps;

internal static class MemoryDumpManager
{
    internal static async Task<string> CollectMemoryDump(int pid, MemoryDumpType dumpType, CancellationToken ct)
    {
        var id = Path.GetRandomFileName();
        var dumpFilename = $"{id}.dmp";
        var dumpFilePath = Path.Combine(Path.GetTempPath(), dumpFilename);

        var type = Map(dumpType);
        var client = new DiagnosticsClient(pid);
        await client.WriteDumpAsync(type, dumpFilePath, false, ct);

        return id;
    }

    private static DumpType Map(MemoryDumpType dumpType) => dumpType switch
    {
        MemoryDumpType.Normal => DumpType.Normal,
        MemoryDumpType.WithHeap => DumpType.WithHeap,
        MemoryDumpType.Triage => DumpType.Triage,
        MemoryDumpType.Full => DumpType.Full,
        _ => throw new ArgumentOutOfRangeException(nameof(dumpType), dumpType, null)
    };
}