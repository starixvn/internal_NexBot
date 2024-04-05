namespace NexBot.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Fanex.BotService.Client.Core.Services;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models.Request;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]")]
    public class RequestController(IMessageService messageService, ILogProviderService logProviderService) : Controller
    {
        [HttpPost]
        [Route("Process")]
        public async Task ProcessRequest(RequestMessage processRequest)
        {
            await messageService.SendRequestAsync(processRequest);
            SendToBotServiceAsync(JsonConvert.SerializeObject(processRequest));
        }

        private void SendToBotServiceAsync(string message)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var botService = new BotService();
                    await botService.SendSkypeAsync("29:1kVSoIVUCQaKlXjtZY627fWWirZiYIgA0Ycv26x6pajg", message);
                }
                catch (Exception exception)
                {
                    logProviderService.LogFileError(exception);
                }
            });

        }
    }
}