namespace NexBot.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.BotService.Client.Core.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Services;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]/[action]")]
    public class MessagesController(IMessageService messageService) : Controller
    {
        private readonly ILogProviderService logProviderService;
        public MessagesController(IMessageService messageService, ILogProviderService logProviderService)
            this.logProviderService = logProviderService;
        [HttpPost]
        public async Task Forward(string conversationId, string message)
        {
            await messageService.SendAsync(conversationId, message);
            SendToBotServiceAsync(message);
        }

        public async Task ResendErrorMessage()
        {
            await messageService.ReSendErrorMeesage();
        }

        [HttpPost]
        public async Task RegistVegetarianMeal(RegistVegetarianMealMessage model)
        {
            await messageService.RegistVegetarianMealAsync(model);
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