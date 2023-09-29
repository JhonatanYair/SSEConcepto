using Queues.AbstracionLayer;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Queues.AbstracionLayer.Enums;
using Newtonsoft.Json;

namespace Queues.RabbitMQ
{
    public class RabbitMQService : IQueueService, IDisposable
    {
        private IConnection connection;
        private IModel channel;
        public event EventHandler<string> messageReceived;
        private Dictionary<string, string> activeConsumers = new Dictionary<string, string>();

        public RabbitMQService()
        {
            var rabbitMQHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            var rabbitMQUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "arixrabbit";
            var rabbitMQPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "YourStrong@Passw0rd";

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQHost,
                Port = 5672,
                UserName = rabbitMQUser, 
                Password = rabbitMQPassword 
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }

        public void DeclareQueue(ExchangeTypes exchange, string toDestination)
        {
            string exchangeName = exchange.ToString();
            string queueName = $"{exchange.ToString()}{toDestination}";
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: queueName);
        }

        public void PublishMessage(ExchangeTypes exchange, PayloadDTO message)
        {
            var properties = channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object>
            {
                { "Tracking", message.tracking.ToString() }
            };

            var payloadMessage = new Payload
            {
                data = message.data,
                creationDate = DateTime.Now,
                eventType = message.eventType.ToString(),
                from = message.from,
                exchange = message.exchange.ToString(),
                to = message.to,
                tracking = message.tracking,
            };

            DeclareQueue(exchange, message.to);
            string exchangeName = exchange.ToString();
            string queueName = $"{exchangeName}{message.to}";
            string messageText = JsonConvert.SerializeObject(payloadMessage);
            var body = Encoding.UTF8.GetBytes(messageText);
            channel.BasicPublish(exchange: exchangeName, routingKey: queueName, basicProperties: properties, body: body);
        }

        public void ConsumeMessage(ExchangeTypes exchange, string toDestination)
        {
            string message = null;
            string trackingId = null;
            string exchangeName = exchange.ToString();
            string queueName = $"{exchangeName}{toDestination}";

            var messageCompletionSource = new TaskCompletionSource<string>();
            DeclareQueue(exchange, toDestination);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                message = Encoding.UTF8.GetString(ea.Body.ToArray());
                //trackingId = ea.BasicProperties.Headers["TrackingId"].ToString();
                OnMessageReceived(message);
            };

            var consumerTag = channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            activeConsumers[queueName] = consumerTag;

        }

        public int GetMessageCount(string queueName)
        {
            var queueDeclareOk = channel.QueueDeclarePassive(queueName);
            return (int)queueDeclareOk.MessageCount;
        }

        public void StopConsumer(string queueName)
        {
            var consumerTag = activeConsumers[queueName];
            channel.BasicCancel(consumerTag);
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
        }


        protected virtual void OnMessageReceived(string message)
        {
            messageReceived?.Invoke(this, message);
        }

    }
}
