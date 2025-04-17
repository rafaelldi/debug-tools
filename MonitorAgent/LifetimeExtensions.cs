using JetBrains.Lifetimes;

namespace MonitorAgent;

internal static class LifetimeExtensions
{
    internal static LifetimeDefinition ToLifetimeDefinition(this CancellationToken cancellationToken)
    {
        var lifetimeDefinition = new LifetimeDefinition();
        cancellationToken.Register(() => lifetimeDefinition.Terminate());
        return lifetimeDefinition;
    }
}