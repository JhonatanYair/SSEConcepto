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
        void DeclareQueue(ExchangeTypes exchange, string subName);
        void PublishMessage(ExchangeTypes exchange, PayloadDTO message);
        void ConsumeMessage(ExchangeTypes exchange, string subName);
        int GetMessageCount(string queueName);
    }
}
