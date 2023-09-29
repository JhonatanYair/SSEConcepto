using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Queues.AbstracionLayer;
using Queues.AbstracionLayer.Enums;

namespace ProducerMessage.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        IQueueService _queueService;

        public ReportController(IQueueService queueService) 
        {
            _queueService = queueService;
        }  
        
        [HttpPost("General")]
        public IActionResult Post(/*[FromBody] */)
        {

            var dataJson = new JObject
            {
                { "report_id", $"{Guid.NewGuid()}" }
            };

            PayloadDTO messageSave = new PayloadDTO()
            {
                from = "ProduccerMessage",
                tracking = Guid.NewGuid(),
                eventType = EventTypes.report_new,
                exchange = ExchangeTypes.ReportService,
                data = dataJson
            };

            _queueService.PublishMessage(Queues.AbstracionLayer.Enums.ExchangeTypes.ReportService,messageSave);   
            return Ok("Publicado");
        }

        [HttpPost("User/{idUser}")]
        public IActionResult PostUser(string idUser)
        {

            var dataJson = new JObject
            {
                { "report_id", $"{Guid.NewGuid()}" }
            };

            PayloadDTO messageSave = new PayloadDTO()
            {
                from = "ProduccerMessage",
                tracking = Guid.NewGuid(),
                eventType = EventTypes.report_new,
                exchange = ExchangeTypes.ReportService,
                to = $"{idUser}",
                data = dataJson
            };

            _queueService.PublishMessage(Queues.AbstracionLayer.Enums.ExchangeTypes.ReportService, messageSave);
            return Ok("Publicado");
        }

    }
}
