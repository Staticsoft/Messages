﻿using Staticsoft.Messages.Abstractions;
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
    public async Task FailsToDequeueMessageTwice()
    {
        var messageBody = "test message";

        await SUT.Enqueue(messageBody);
        var message = await SUT.Dequeue(CancellationToken.None);

        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => SUT.Dequeue(cancellation.Token)
        );
    }

    [Fact]
    public async Task DequeuesMessageOnSetVisibility()
    {
        await SUT.Enqueue("test message");
        var message = await SUT.Dequeue(CancellationToken.None);
        var visibleAt = DateTime.UtcNow.AddSeconds(7);

        await SUT.ResetVisibility(message.Id, visibleAt);

        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => SUT.Dequeue(cancellation.Token)
        );

        cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var visibleMessage = await SUT.Dequeue(cancellation.Token);
        Assert.Equal(message.Body, visibleMessage.Body);
    }

    [Fact]
    public async Task FailsToDequeueDeletedMessage()
    {
        await SUT.Enqueue("test message");
        var message = await SUT.Dequeue(CancellationToken.None);

        await SUT.ResetVisibility(message.Id, DateTime.UtcNow);
        await SUT.Delete(message.Id);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => SUT.Dequeue(cts.Token)
        );
    }

    public async Task InitializeAsync()
    {
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (!cancellation.Token.IsCancellationRequested)
            {
                var message = await SUT.Dequeue(cancellation.Token);
                await SUT.Delete(message.Id);
            }
        });
    }

    public Task DisposeAsync()
        => Task.CompletedTask;
}
