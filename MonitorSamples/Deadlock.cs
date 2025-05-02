// ReSharper disable InconsistentOrderOfLocks

namespace MonitorSamples;

internal static class Deadlock
{
    private static readonly Lock Lock1 = new();
    private static readonly Lock Lock2 = new();

    internal static void MapDeadlockEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/deadlock", async (ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Deadlock");
            var task1 = Task.Run(() =>
            {
                lock (Lock1)
                {
                    logger.LogInformation("Task 1 - After Lock1");
                    Thread.Sleep(1000);
                    lock (Lock2)
                    {
                        logger.LogInformation("Task 1 - After Lock2");
                    }
                }
            });

            var task2 = Task.Run(() =>
            {
                lock (Lock2)
                {
                    logger.LogInformation("Task 2 - After Lock2");
                    Thread.Sleep(1000);
                    lock (Lock1)
                    {
                        logger.LogInformation("Task 2 - After Lock1");
                    }
                }
            });

            await Task.WhenAll(task1, task2);
        });
    }
}