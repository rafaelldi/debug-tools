using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Stacks;
using MonitorAgent;
using static Monitor.Common.ProviderNames;
using Thread = MonitorAgent.Thread;

namespace Monitor.ThreadDumps;

internal static class ThreadDumpManager
{
    private const string ThreadFrameName = "Thread (";

    internal static async Task<ThreadDump> CollectThreadDump(int pid, CancellationToken ct)
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
        var threadDump = ParseSessionFile(traceLogFilePath);

        return threadDump;
    }

    private static ThreadDump ParseSessionFile(string traceLogFilePath)
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

        var stackTraces = BuildThreadDumpFromSamples(samplesByThread, stackSource);

        return stackTraces;
    }

    private static ThreadDump BuildThreadDumpFromSamples(Dictionary<int, List<StackSourceSample>> samplesByThread,
        MutableTraceEventStackSource stackSource)
    {
        var threadDump = new ThreadDump();

        foreach (var threadSamples in samplesByThread)
        {
            var sample = threadSamples.Value.FirstOrDefault();
            if (sample is null) continue;

            var thread = new Thread
            {
                ThreadId = $"Thread (0x{threadSamples.Key:X})"
            };
            threadDump.Treads.Add(thread);

            var stackIndex = sample.StackIndex;
            var frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            while (!frameName.StartsWith(ThreadFrameName))
            {
                var frame = frameName != "UNMANAGED_CODE_TIME" ? frameName : "[Native Frames]";
                thread.Frames.Add(frame);

                stackIndex = stackSource.GetCallerIndex(stackIndex);
                frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            }
        }

        return threadDump;
    }
}