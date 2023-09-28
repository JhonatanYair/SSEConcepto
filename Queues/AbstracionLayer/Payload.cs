using Newtonsoft.Json.Linq;
using Queues.AbstracionLayer.Enums;
using System.Runtime.Serialization;

namespace Queues.AbstracionLayer
{
    public class Payload
    {
        public int Event_id { get; set; }
        public Guid tracking { get; set; }
        public string From { get; set; }
        public string? To { get; set; }
        public string EventType { get; set; }
        public string Exchange { get; set; }
        public DateTime? DateCreate { get; set; }
        public JObject Data { get; set; }
    }

    public class PayloadDTO
    {
        public Guid tracking { get; set; }
        public string From { get; set; }
        public string? To { get; set; }
        public EventTypes EventType { get; set; }
        public ExchangeTypes Exchange { get; set; }
        public JObject Data { get; set; }
    }

}
