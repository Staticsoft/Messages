namespace Staticsoft.Messages.Abstractions;

public interface Queue
{
    Task Enqueue(string body);
    Task<Message> Dequeue()
        => Dequeue(CancellationToken.None);
    Task<Message> Dequeue(CancellationToken cancellation);
    Task ResetVisibility(string messageId, DateTime visibleAt);
    Task Delete(string messageId);

    public class Message
    {
        public required string Id { get; init; }
        public required string Body { get; init; }
    }
}
