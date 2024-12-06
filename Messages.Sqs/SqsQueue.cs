using Amazon.SQS;
using Amazon.SQS.Model;
using Staticsoft.Messages.Abstractions;

namespace Staticsoft.Messages.Sqs;

public class SqsQueue(
    IAmazonSQS sqs,
    SqsQueueOptions options
) : Queue
{
    readonly IAmazonSQS Sqs = sqs;
    readonly string QueueUrl = options.QueueUrl;
    readonly int InvisibilitySeconds = (int)options.Invisibility.TotalSeconds;

    public async Task Enqueue(string body)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = QueueUrl,
            MessageBody = body
        };

        await Sqs.SendMessageAsync(request);
    }

    public async Task<Queue.Message> Dequeue(CancellationToken cancellation)
    {
        var request = new ReceiveMessageRequest
        {
            QueueUrl = QueueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 20,
            VisibilityTimeout = InvisibilitySeconds
        };

        var response = await Sqs.ReceiveMessageAsync(request, cancellation);

        var message = response.Messages.Single();
        return new Queue.Message
        {
            Id = message.ReceiptHandle,
            Body = message.Body
        };
    }

    public async Task ResetVisibility(string messageId, DateTime visibleAt)
    {
        var visibilityTimeout = (int)Math.Ceiling((visibleAt - DateTime.UtcNow).TotalSeconds);
        if (visibilityTimeout < 0)
            visibilityTimeout = 0;

        var request = new ChangeMessageVisibilityRequest
        {
            QueueUrl = QueueUrl,
            ReceiptHandle = messageId,
            VisibilityTimeout = visibilityTimeout
        };

        await Sqs.ChangeMessageVisibilityAsync(request);
    }

    public async Task Delete(string messageId)
    {
        var request = new DeleteMessageRequest
        {
            QueueUrl = QueueUrl,
            ReceiptHandle = messageId
        };

        await Sqs.DeleteMessageAsync(request);
    }
}
