using Staticsoft.Messages.Abstractions;
using System.Collections.Concurrent;

namespace Staticsoft.Messages.Memory;

public class MemoryQueue(
    MemoryQueueOptions options
) : Queue
{
    class VisibleMessage
    {
        public DateTime VisibleAt { get; set; } = DateTime.UtcNow;
        public required Queue.Message Data { get; init; }
    }

    readonly MemoryQueueOptions Options = options;
    readonly ConcurrentDictionary<string, VisibleMessage> Messages = [];

    public Task Enqueue(string body)
    {
        var id = $"{Guid.NewGuid()}";
        Messages[id] = new() { Data = new() { Id = id, Body = body } };

        return Task.CompletedTask;
    }

    public async Task<Queue.Message> Dequeue(CancellationToken cancellation)
    {
        var message = GetMessage();
        while (message == null)
        {
            await Task.Delay(1000, cancellation);
            message = GetMessage();
        }

        message.VisibleAt = DateTime.UtcNow + Options.Invisibility;
        return message.Data;
    }

    VisibleMessage? GetMessage()
        => Messages.Values
            .Where(message => message.VisibleAt < DateTime.UtcNow)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

    public Task ResetVisibility(string messageId, DateTime visibleAt)
    {
        Messages[messageId].VisibleAt = visibleAt;

        return Task.CompletedTask;
    }

    public Task Delete(string messageId)
    {
        Messages.TryRemove(messageId, out var _);

        return Task.CompletedTask;
    }
}
