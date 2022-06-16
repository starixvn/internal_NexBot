namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using NexBot.Core.Services.Interfaces;

    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;
        private readonly ILogProviderService logger;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot, ILogProviderService logger)
        {
            Adapter = adapter;
            Bot = bot;
            this.logger = logger;
        }

        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            logger.WriteLogFileInfo("Received message from outside");
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}