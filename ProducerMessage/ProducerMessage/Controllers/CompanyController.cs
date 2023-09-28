using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Queues.AbstracionLayer;
using Queues.AbstracionLayer.Enums;

namespace ProducerMessage.Controllers
{
    [Route("api/[controller]")]
    public class CompanyController : Controller
    {

        IQueueService _queueService;

        public CompanyController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpPost("General")]
        public IActionResult Post(/*[FromBody] */)
        {

            var dataJson = new JObject
            {
                { "company_id", $"{Guid.NewGuid()}" }
            };

            PayloadDTO messageSave = new PayloadDTO()
            {
                From = "ProduccerMessage",
                tracking = Guid.NewGuid(),
                EventType = EventTypes.company_new,
                Exchange = ExchangeTypes.CompanyService,
                Data = dataJson
            };

            _queueService.PublishMessage(Queues.AbstracionLayer.Enums.ExchangeTypes.CompanyService, messageSave);
            return Ok("Publicado");
        }

        [HttpPost("User/{idUser}")]
        public IActionResult PostUser(string idUser)
        {

            var dataJson = new JObject
            {
                { "company_id", $"{Guid.NewGuid()}" }
            };

            PayloadDTO messageSave = new PayloadDTO()
            {
                From = "ProduccerMessage",
                tracking = Guid.NewGuid(),
                EventType = EventTypes.company_new,
                Exchange = ExchangeTypes.CompanyService,
                To = $"{idUser}",
                Data = dataJson
            };

            _queueService.PublishMessage(Queues.AbstracionLayer.Enums.ExchangeTypes.CompanyService, messageSave);
            return Ok("Publicado");
        }

    }
}
