// ReSharper disable InconsistentOrderOfLocks
// ReSharper disable ChangeFieldTypeToSystemThreadingLock

namespace MonitorSamples.Deadlock;

internal static class Deadlock
{
    private static readonly object ResourceA = new();
    private static readonly object ResourceB = new();

    internal static void MapDeadlockEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/deadlock", async (HttpContext context) =>
        {
            var backgroundTask = Task.Run(() =>
            {
                lock (ResourceB)
                {
                    Thread.Sleep(500);

                    lock (ResourceA)
                    {
                        return "Background task completed";
                    }
                }
            });

            await Task.Delay(100);

            var mainTask = Task.Run(() =>
            {
                lock (ResourceA)
                {
                    Thread.Sleep(500);

                    lock (ResourceB)
                    {
                        return "Main thread completed";
                    }
                }
            });

            await Task.WhenAll(mainTask, backgroundTask);

            return "All tasks completed";
        });
    }
}