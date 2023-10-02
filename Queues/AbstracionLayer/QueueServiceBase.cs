using log4net;
using Newtonsoft.Json;
using Queues.AbstracionLayer.Enums;
using Queues.RabbitMQ;

namespace Queues.AbstracionLayer;

public class QueueServiceBase : IQueueService
{

    private static readonly ILog _logger = LogManager.GetLogger(typeof(QueueServiceBase));
    RabbitMQService messageQueueService;
    public event Action<object, string> onMessageReceived;
    private List<Action<object, string>> onMessageReceivedSubscribers = new List<Action<object, string>>();
    public bool IsQueueInitialized = false;

    public QueueServiceBase()
    {
        _logger.Debug("IN - QueueServiceBase()");
        try
        {
            messageQueueService = new RabbitMQService();
            IsQueueInitialized = true;
            messageQueueService.messageReceived += (sender, message) =>
            {
                foreach (var subscriber in onMessageReceivedSubscribers)
                {
                    subscriber(sender, message);
                }
            };
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    public void DeclareQueue(ExchangeTypes exchange, string toDestination)
    {
        _logger.Debug("IN - DeclareQueue()");
        try
        {
            messageQueueService.DeclareQueue(exchange, toDestination);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    public void PublishMessage(ExchangeTypes exchange, PayloadDTO message)
    {
        _logger.Debug("IN - PublishMessage()");
        try
        {
            messageQueueService.PublishMessage(exchange, message);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    public void ConsumeMessage(ExchangeTypes exchange, string toDestination)
    {
        _logger.Debug("IN - ConsumeMessage()");
        try
        {
            if (messageQueueService != null)
            {
                messageQueueService.messageReceived += (sender, message) => onMessageReceived?.Invoke(sender, message);
                messageQueueService.ConsumeMessage(exchange, toDestination);
            }
            else
            {
                _logger.Error("The event consumer has no message.");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    public int GetMessageCount(string queueName)
    {
        return messageQueueService.GetMessageCount(queueName);
    }

    public void SubscribeToMessage(Action<object, string> subscriber)
    {
        _logger.Debug("IN - SubscribeToMessage()");
        onMessageReceivedSubscribers.Add(subscriber);
    }

    public void UnsubscribeFromMessage(Action<object, string> subscriber, string queueName)
    {
        _logger.Debug("IN - UnsubscribeFromMessage()");
        try
        {
            onMessageReceivedSubscribers.Remove(subscriber);
            messageQueueService.StopConsumer(queueName);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }

    }

}