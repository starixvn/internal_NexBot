namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Models.Request;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class RequestController(IMessageService messageService) : Controller
    {
        [HttpPost]
        [Route("Process")]
        public async Task ProcessRequest(RequestMessage processRequest)
        {
            await messageService.SendRequestAsync(processRequest);
        }
    }
}