using Microsoft.Extensions.DependencyInjection;
using Queues.AbstracionLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queues.RabbitMQ
{
    public static class Extensions
    {
        public static IServiceCollection AddQueuesStreaming(this IServiceCollection services)
            => services
                .AddSingleton<IQueueService, QueueServiceBase>();
                //.AddSingleton<IQueueService, RabbitMQMessageQueueService>();
    }
}
