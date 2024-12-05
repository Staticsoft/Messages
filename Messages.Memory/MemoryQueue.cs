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

    readonly SemaphoreSlim Lock = new(1, 1);
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

        await Lock.WaitAsync(cancellation);

        message.VisibleAt += Options.Invisibility;
        Lock.Release();
        return message.Data;
    }

    VisibleMessage? GetMessage()
        => Messages.Values
            .Where(message => message.VisibleAt < DateTime.UtcNow)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

    public async Task ResetVisibility(string messageId, DateTime visibleAt)
    {
        await Lock.WaitAsync();
        Messages[messageId].VisibleAt = visibleAt;
        Lock.Release();
    }

    public async Task Delete(string messageId)
    {
        await Lock.WaitAsync();
        Messages.TryRemove(messageId, out var _);
        Lock.Release();
    }
}
