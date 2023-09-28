using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Queues.AbstracionLayer.Enums;
using Queues.RabbitMQ;

namespace Queues.AbstracionLayer;

public class QueueServiceBase : IQueueService
{
    RabbitMQService messageQueueService;
    public event Action<object, string> OnMessageReceived;
    private List<Action<object, string>> onMessageReceivedSubscribers = new List<Action<object, string>>();

    public QueueServiceBase()
    {
        messageQueueService = new RabbitMQService();

        messageQueueService.MessageReceived += (sender, message) =>
        {
            foreach (var subscriber in onMessageReceivedSubscribers)
            {
                subscriber(sender, message);
            }
        };
    }

    public void DeclareQueue(ExchangeTypes exchange)
    {
        messageQueueService.DeclareQueue(exchange);
    }

    public void PublishMessage(ExchangeTypes exchange, PayloadDTO message)
    {
        messageQueueService.PublishMessage(exchange, message);
    }

    public string ConsumeMessage(ExchangeTypes exchange)
    {
        messageQueueService.MessageReceived += (sender, message) => OnMessageReceived?.Invoke(sender, message);
        var message = messageQueueService.ConsumeMessage(exchange);
        return message;
    }

    public int GetMessageCount(string queueName)
    {
        return messageQueueService.GetMessageCount(queueName);
    }

    public void SubscribeToMessageReceived(Action<object, string> subscriber)
    {
        onMessageReceivedSubscribers.Add(subscriber);
        Console.WriteLine();
    }

    public void UnsubscribeFromMessageReceived(Action<object, string> subscriber)
    {
        onMessageReceivedSubscribers.Remove(subscriber);
    }

}
