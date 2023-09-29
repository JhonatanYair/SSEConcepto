using Newtonsoft.Json.Linq;
using Queues.AbstracionLayer.Enums;
using System.Runtime.Serialization;

namespace Queues.AbstracionLayer
{
    public class Payload
    {
        public int event_id { get; set; }
        public Guid tracking { get; set; }
        public string from { get; set; }
        public string? to { get; set; }
        public string eventType { get; set; }
        public string exchange { get; set; }
        public DateTime? creationDate { get; set; }
        public JObject data { get; set; }
    }

    public class PayloadDTO
    {
        public Guid tracking { get; set; }
        public string from { get; set; }
        public string? to { get; set; } = "";
        public EventTypes eventType { get; set; }
        public ExchangeTypes exchange { get; set; }
        public JObject data { get; set; }

    }

}
