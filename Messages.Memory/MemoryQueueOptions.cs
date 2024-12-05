namespace Staticsoft.Messages.Memory;

public class MemoryQueueOptions
{
    public TimeSpan Invisibility { get; init; } = TimeSpan.FromMinutes(1);
}
