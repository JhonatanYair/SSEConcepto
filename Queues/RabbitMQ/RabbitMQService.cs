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
        public event EventHandler<string> MessageReceived;

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

        public void DeclareQueue(ExchangeTypes exchange)
        {
            string exchangeName = exchange.ToString();
            string queueName = exchange.ToString();

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            // Declarar una cola para el ServicioA
            channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            // Vincular la cola al exchange para el ServicioA
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");
        }

        public void PublishMessage(ExchangeTypes exchange, PayloadDTO message)
        {
            var properties = channel.CreateBasicProperties();

            // Asignar el tracking ID a la cabecera del mensaje
            properties.Headers = new Dictionary<string, object>
            {
                { "TrackingId", message.tracking.ToString() }
            };

            var payloadMessage = new Payload
            {
                Data = message.Data,
                DateCreate = DateTime.Now,
                EventType = message.EventType.ToString(),
                From = message.From,
                Exchange = message.Exchange.ToString(),
                To = message.To,
                tracking = message.tracking,
            };

            string exchangeName = exchange.ToString();
            DeclareQueue(exchange);
            string messageText = JsonConvert.SerializeObject(payloadMessage);
            var body = Encoding.UTF8.GetBytes(messageText);
            channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: properties, body: body);
        }

        public string ConsumeMessage(ExchangeTypes exchange)
        {
            string message = null;
            string trackingId = null;
            string exchangeName = exchange.ToString();
            string queueName = exchange.ToString();

            var messageCompletionSource = new TaskCompletionSource<string>();
            DeclareQueue(exchange);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                message = Encoding.UTF8.GetString(ea.Body.ToArray());
                //trackingId = ea.BasicProperties.Headers["TrackingId"].ToString();
                OnMessageReceived(message);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            Console.WriteLine();

            return message;

        }

        public int GetMessageCount(string queueName)
        {
            var queueDeclareOk = channel.QueueDeclarePassive(queueName);
            return (int)queueDeclareOk.MessageCount;
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
        }

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

    }
}
