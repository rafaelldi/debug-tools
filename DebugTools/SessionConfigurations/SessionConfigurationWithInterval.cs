namespace Monitor.SessionConfigurations;

internal abstract class SessionConfigurationWithInterval(int processId, int? refreshInterval)
    : BaseSessionConfiguration(processId)
{
    private const int DefaultInterval = 1;

    internal int RefreshInterval { get; } = refreshInterval > 0 ? refreshInterval.Value : DefaultInterval;
}