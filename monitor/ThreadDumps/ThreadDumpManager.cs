using System.Diagnostics.Tracing;
using System.Text;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Stacks;
using static Monitor.Common.ProviderNames;

namespace Monitor.ThreadDumps;

internal static class ThreadDumpManager
{
    private const string ThreadFrameName = "Thread (";

    internal static async Task<string> CollectThreadDump(int pid, CancellationToken ct)
    {
        var dumpFilename = $"{Path.GetRandomFileName()}.nettrace";
        var dumpFilePath = Path.Combine(Path.GetTempPath(), dumpFilename);

        var client = new DiagnosticsClient(pid);
        var providers = new List<EventPipeProvider>
        {
            new(MicrosoftDotNetCoreSampleProfiler, EventLevel.Informational)
        };

        using (var session = await client.StartEventPipeSessionAsync(providers, requestRundown: true, token: ct))
        {
            await using (var fileStream = new FileStream(dumpFilePath, FileMode.Create, FileAccess.Write))
            {
                var copyToFileTask = session.EventStream.CopyToAsync(fileStream, ct);
                await Task.Delay(TimeSpan.FromMilliseconds(10), ct);
                await session.StopAsync(ct);
                await copyToFileTask;
            }
        }

        var traceLogFilePath = TraceLog.CreateFromEventPipeDataFile(dumpFilePath);
        var stackTraces = ParseSessionFile(traceLogFilePath);

        return stackTraces;
    }

    private static string ParseSessionFile(string traceLogFilePath)
    {
        using var symbolReader = new SymbolReader(TextWriter.Null);
        symbolReader.SymbolPath = SymbolPath.MicrosoftSymbolServerPath;

        using var traceLog = new TraceLog(traceLogFilePath);
        var stackSource = new MutableTraceEventStackSource(traceLog)
        {
            OnlyManagedCodeStacks = true
        };

        var computer = new SampleProfilerThreadTimeComputer(traceLog, symbolReader);
        computer.GenerateThreadTimeStacks(stackSource);

        var samplesByThread = new Dictionary<int, List<StackSourceSample>>();
        stackSource.ForEach(it =>
        {
            var stackIndex = it.StackIndex;
            var frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            while (!frameName.StartsWith(ThreadFrameName))
            {
                stackIndex = stackSource.GetCallerIndex(stackIndex);
                frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            }

            var threadId = int.Parse(frameName[8..^1]);

            if (samplesByThread.TryGetValue(threadId, out var samples))
            {
                samples.Add(it);
            }
            else
            {
                samplesByThread[threadId] = [it];
            }
        });

        var stackTraces = SerializeStackTraces(samplesByThread, stackSource);

        return stackTraces;
    }

    private static string SerializeStackTraces(Dictionary<int, List<StackSourceSample>> samplesByThread,
        MutableTraceEventStackSource stackSource)
    {
        var sb = new StringBuilder();

        foreach (var threadSamples in samplesByThread)
        {
            var sample = threadSamples.Value.FirstOrDefault();
            if (sample is null) continue;

            sb.AppendLine($"Thread (0x{threadSamples.Key:X}):");

            var stackIndex = sample.StackIndex;
            var frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            while (!frameName.StartsWith(ThreadFrameName))
            {
                sb.AppendLine(frameName != "UNMANAGED_CODE_TIME" ? $"    {frameName}" : "    [Native Frames]");
                stackIndex = stackSource.GetCallerIndex(stackIndex);
                frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}