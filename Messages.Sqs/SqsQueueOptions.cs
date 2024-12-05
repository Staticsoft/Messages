namespace Staticsoft.Messages.Sqs;

public class SqsQueueOptions
{
    public required string QueueUrl { get; init; }
    public TimeSpan Invisibility { get; init; } = TimeSpan.FromMinutes(1);
}
