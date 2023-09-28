using Queues.AbstracionLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queues.AbstracionLayer
{
    public interface IQueueService
    {
        void DeclareQueue(ExchangeTypes exchange);
        void PublishMessage(ExchangeTypes exchange, PayloadDTO message);
        string ConsumeMessage(ExchangeTypes exchange);
        int GetMessageCount(string queueName);
    }
}
