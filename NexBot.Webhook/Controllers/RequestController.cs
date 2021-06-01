namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class RequestController : Controller
    {
        private readonly IMessageService messageService;

        public RequestController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        //[Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Approve")]
        public async Task ApproveRequest(SendRequestMessage processRequest)
        {
            await messageService.AddSystemLog("Approve request");
            await messageService.SendRequestAsync(processRequest);
        }
    }
}