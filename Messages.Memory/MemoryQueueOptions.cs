namespace Staticsoft.Messages.Memory;

public class MemoryQueueOptions
{
    public TimeSpan Invisibility { get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(1);
}
