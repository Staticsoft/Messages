using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Messages.Sqs;

namespace Staticsoft.Messages.Tests;

public class SqsMessagesTests : QueueTests
{
    protected override IServiceCollection Services => base.Services
        .UseSqsQueue(
            _ => new AmazonSQSClient(GetAccessKeyId(), GetSecretAccessKey(), GetRegion()),
            _ => new() { Invisibility = Invisibility, QueueUrl = GetQueueUrl() }
        );

    static string GetQueueUrl()
        => EnvVariable("MessagesQueueUrl");

    static string GetAccessKeyId()
        => EnvVariable("MessagesAccessKeyId")!;

    static string GetSecretAccessKey()
        => EnvVariable("MessagesSecretAccessKey")!;

    static RegionEndpoint GetRegion()
        => RegionEndpoint.GetBySystemName(EnvVariable("MessagesRegion"));

    static string EnvVariable(string name)
        => Environment.GetEnvironmentVariable(name)
        ?? throw new ArgumentNullException($"Environment variable {name} is not set");
}