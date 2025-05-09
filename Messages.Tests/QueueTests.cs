using Staticsoft.Messages.Abstractions;
using Staticsoft.Testing;
using Xunit;

namespace Staticsoft.Messages.Tests;

public abstract class QueueTests : TestBase<Queue>, IAsyncLifetime
{
    protected static readonly TimeSpan Invisibility = TimeSpan.FromSeconds(3);

    [Fact]
    public async Task FailsToDequeueNonExistingMessage()
    {
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => SUT.Dequeue(cancellation.Token)
        );
    }

    [Fact]
    public async Task DequeuesEnqueuedMessage()
    {
        var messageBody = "test message";

        await SUT.Enqueue(messageBody);
        var message = await SUT.Dequeue(CancellationToken.None);

        Assert.Equal(messageBody, message.Body);
    }

    [Fact]
    public async Task DequeuesMessageOnSetVisibility()
    {
        await SUT.Enqueue("test message");
        var message = await SUT.Dequeue(CancellationToken.None);
        var visibleAt = DateTime.UtcNow.AddSeconds(10);

        await SUT.ResetVisibility(message.Id, visibleAt);

        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => SUT.Dequeue(cancellation.Token)
        );

        cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var visibleMessage = await SUT.Dequeue(cancellation.Token);
        Assert.Equal(message.Body, visibleMessage.Body);
    }

    [Fact]
    public async Task FailsToDequeueDeletedMessage()
    {
        await SUT.Enqueue("test message");
        var message = await SUT.Dequeue(CancellationToken.None);

        await SUT.Delete(message.Id);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => SUT.Dequeue(cts.Token)
        );
    }

    [Fact]
    public async Task FailsToDequeueMessageInvisibleMessage()
    {
        await SUT.Enqueue("test message");
        await Task.Delay(Invisibility * 2);

        await SUT.Dequeue(CancellationToken.None);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            var cancellation = new CancellationTokenSource(Invisibility * 0.5);
            await SUT.Dequeue(cancellation.Token);
        });
    }

    public async Task InitializeAsync()
    {
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (true)
            {
                var message = await SUT.Dequeue(cancellation.Token);
                await SUT.Delete(message.Id);
            }
        });
    }

    public Task DisposeAsync()
        => Task.CompletedTask;
}
