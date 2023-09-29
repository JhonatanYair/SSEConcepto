using Queues.AbstracionLayer.Enums;
using Queues.RabbitMQ;

namespace Queues.AbstracionLayer;

public class QueueServiceBase : IQueueService
{
    RabbitMQService messageQueueService;
    public event Action<object, string> onMessageReceived;
    private List<Action<object, string>> onMessageReceivedSubscribers = new List<Action<object, string>>();

    public QueueServiceBase()
    {
        messageQueueService = new RabbitMQService();

        messageQueueService.messageReceived += (sender, message) =>
        {
            foreach (var subscriber in onMessageReceivedSubscribers)
            {
                subscriber(sender, message);
            }
        };
    }

    public void DeclareQueue(ExchangeTypes exchange, string toDestination)
    {
        messageQueueService.DeclareQueue(exchange, toDestination);
    }

    public void PublishMessage(ExchangeTypes exchange, PayloadDTO message)
    {
        messageQueueService.PublishMessage(exchange, message);
    }

    public void ConsumeMessage(ExchangeTypes exchange, string toDestination)
    {
        messageQueueService.messageReceived += (sender, message) => onMessageReceived?.Invoke(sender, message);
        messageQueueService.ConsumeMessage(exchange, toDestination);
    }

    public int GetMessageCount(string queueName)
    {
        return messageQueueService.GetMessageCount(queueName);
    }

    public void SubscribeToMessage(Action<object, string> subscriber)
    {
        onMessageReceivedSubscribers.Add(subscriber);
    }

    public void UnsubscribeFromMessage(Action<object, string> subscriber, string queueName)
    {
        onMessageReceivedSubscribers.Remove(subscriber);
        messageQueueService.StopConsumer(queueName);
    }

}
