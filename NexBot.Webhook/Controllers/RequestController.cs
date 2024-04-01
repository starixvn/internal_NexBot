namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Models.Request;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class RequestController : Controller
    {
        private readonly IMessageService messageService;

        public RequestController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        [HttpPost]
        [Route("Process")]
        public async Task ProcessCheckInOut(RequestMessage processRequest)
        {
            processRequest.AvatarUrl = processRequest.AvatarUrl.Replace("https://localhost:44312", "https://hrm.nexdev.net");
            await messageService.SendRequestAsync(processRequest);
        }
    }
}