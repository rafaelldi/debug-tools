using System.Diagnostics;
using System.Globalization;
using Microsoft.Diagnostics.NETCore.Client;

namespace MonitorAgent.Processes;

internal sealed class ProcessHandler
{
    private const string MonitorAgentProcessName = "MonitorAgent";

    internal List<ProcessInfo> GetProcessList()
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

    internal ProcessDetails? GetProcessDetails(int pid)
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

    internal List<ProcessEnvironment> GetProcessEnvironment(int pid)
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