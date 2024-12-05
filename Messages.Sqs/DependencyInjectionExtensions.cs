using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Messages.Abstractions;

namespace Staticsoft.Messages.Sqs;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseSqsQueue(
        this IServiceCollection services,
        Func<IServiceProvider, IAmazonSQS> sqs,
        Func<IServiceProvider, SqsQueueOptions> options
    )
        => services
            .AddSingleton<Queue, SqsQueue>()
            .AddSingleton(sqs)
            .AddSingleton(options);
}
