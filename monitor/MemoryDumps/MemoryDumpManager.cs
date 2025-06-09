using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Runtime;
using MonitorAgent;

namespace Monitor.MemoryDumps;

internal static class MemoryDumpManager
{
    internal static async Task<string> CollectMemoryDump(int pid, MemoryDumpType dumpType, CancellationToken token)
    {
        var id = Path.GetRandomFileName();
        var dumpFilename = $"{id}.dmp";
        var dumpFilePath = Path.Combine(Path.GetTempPath(), dumpFilename);

        var type = Map(dumpType);
        var client = new DiagnosticsClient(pid);
        await client.WriteDumpAsync(type, dumpFilePath, false, token);

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

    internal static void DeleteMemoryDump(string memoryDumpId)
    {
        var dumpFilename = $"{memoryDumpId}.dmp";
        var dumpFilePath = Path.Combine(Path.GetTempPath(), dumpFilename);
        if (File.Exists(dumpFilePath))
        {
            File.Delete(dumpFilePath);
        }
    }

    internal static void GetClrStack(string id)
    {
        var dumpFilename = $"{id}.dmp";
        var dumpFilePath = Path.Combine(Path.GetTempPath(), dumpFilename);
        var dataTarget = DataTarget.LoadDump(dumpFilePath, new CacheOptions());
        var clrInfo = dataTarget.ClrVersions[0];
        var runtime = clrInfo.CreateRuntime();
        var threads = runtime.Threads
            .Select(it => (it, it.EnumerateStackTrace()))
            .ToList();
    }
}