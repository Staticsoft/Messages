using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Messages.Memory;

namespace Staticsoft.Messages.Tests;

public class MemoryMessagesTests : QueueTests
{
    protected override IServiceCollection Services => base.Services
        .UseMemoryQueue(_ => new MemoryQueueOptions() { Invisibility = Invisibility });
}
