using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Messages.Abstractions;

namespace Staticsoft.Messages.Memory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseMemoryQueue(
        this IServiceCollection services,
        Func<IServiceProvider, MemoryQueueOptions> options
    )
        => services
            .AddSingleton<Queue, MemoryQueue>()
            .AddSingleton(options);
}
