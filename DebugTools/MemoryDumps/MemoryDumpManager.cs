using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Runtime;
using MonitorAgent;
using ClrThread = MonitorAgent.ClrThread;

namespace Monitor.MemoryDumps;

internal static class MemoryDumpManager
{
    internal static async Task<string> CollectMemoryDump(int pid, MemoryDumpType dumpType, CancellationToken token)
    {
        var memoryDumpId = Path.GetRandomFileName();
        var dumpFilePath = GetDumpFilePath(memoryDumpId);
        var type = Map(dumpType);
        var client = new DiagnosticsClient(pid);
        await client.WriteDumpAsync(type, dumpFilePath, false, token);

        return memoryDumpId;
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
        var dumpFilePath = GetDumpFilePath(memoryDumpId);
        if (File.Exists(dumpFilePath))
        {
            File.Delete(dumpFilePath);
        }
    }

    internal static ClrStack GetClrStack(string memoryDumpId)
    {
        var dumpFilePath = GetDumpFilePath(memoryDumpId);
        var dataTarget = DataTarget.LoadDump(dumpFilePath, new CacheOptions());
        var clrInfo = dataTarget.ClrVersions[0];
        var runtime = clrInfo.CreateRuntime();
        var threads = runtime.Threads
            .Select(it => (Thread: it, StackTrace: it.EnumerateStackTrace()))
            .ToList();
        var clrThreads = new List<ClrThread>(threads.Count);

        foreach (var thread in threads)
        {
            var clrThread = new ClrThread
            {
                ThreadId = thread.Thread.ManagedThreadId
            };
            var threadStack = thread.StackTrace
                .Select(it => it.Method?.Signature ?? it.FrameName ?? "unknown");
            clrThread.Frames.AddRange(threadStack);

            clrThreads.Add(clrThread);
        }

        var stack = new ClrStack();
        stack.Treads.AddRange(clrThreads.OrderBy(it => it.ThreadId).Where(it => it.Frames.Count > 0));

        return stack;
    }

    private static string GetDumpFilePath(string memoryDumpId)
    {
        var dumpFilename = $"{memoryDumpId}.dmp";
        return Path.Combine(Path.GetTempPath(), dumpFilename);
    }
}