using System.Diagnostics;
using System.Globalization;
using Microsoft.Diagnostics.NETCore.Client;
using MonitorAgent;

namespace Monitor.Processes;

internal static class ProcessManager
{
    private const string MonitorAgentProcessName = "DebugTools";

    internal static List<ProcessInfo> GetProcessList()
    {
        var processIds = DiagnosticsClient.GetPublishedProcesses().ToList();
        var result = new List<ProcessInfo>(processIds.Count);
        foreach (var pid in processIds)
        {
            var process = Process.GetProcessById(pid);
            if (process.ProcessName == MonitorAgentProcessName) continue;

            result.Add(new ProcessInfo
            {
                ProcessId = pid,
                ProcessName = process.ProcessName
            });
        }

        return result;
    }

    internal static ProcessInfo? GetProcessByName(string processName)
    {
        var processList = GetProcessList();
        return processList.FirstOrDefault(it => it.ProcessName == processName);
    }

    internal static ProcessDetails? GetProcessDetails(int pid)
    {
        try
        {
            var client = new DiagnosticsClient(pid);
            var process = Process.GetProcessById(pid);
            var filename = process.MainModule?.FileName;
            var startTime = process.StartTime.ToString(CultureInfo.InvariantCulture);
            var additionalProcessInfo = client.GetProcessInfo();

            return new ProcessDetails
            {
                ProcessId = pid,
                Filename = filename,
                StartTime = startTime,
                CommandLine = additionalProcessInfo.CommandLine,
                OperatingSystem = additionalProcessInfo.OperatingSystem,
                ProcessArchitecture = additionalProcessInfo.ProcessArchitecture
            };
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    internal static List<ProcessEnvironment> GetProcessEnvironment(int pid)
    {
        try
        {
            var client = new DiagnosticsClient(pid);
            return client
                .GetProcessEnvironment()
                .Select(it => new ProcessEnvironment
                {
                    Key = it.Key,
                    Value = it.Value
                })
                .ToList();
        }
        catch (ArgumentException)
        {
            return [];
        }
    }
}