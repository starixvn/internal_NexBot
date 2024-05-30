namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]/[action]")]
    public class MessagesController(IMessageService messageService, ILogProviderService logService) : Controller
    {
        [HttpPost]
        public async Task Forward(string conversationId, string message)
        {
            await messageService.SendAsync(conversationId, message);
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

        [HttpPost]
        public async Task CheckoutNotification(CheckoutRecord record)
        {
            try
            {
                if (record is not null)
                {
                    await messageService.CheckoutNotificationAsync(record);
                }
            }
            catch (System.Exception ex)
            {
                await logService.SentryErrorAsync(JsonConvert.SerializeObject(record));
            }
        }

        [HttpPost]
        public async Task ReachedWorkingHoursNotification(ReachedWorkingHours reachedWorkingHours)
        {
            try
            {
                if (reachedWorkingHours is not null)
                {
                    await messageService.ReachedWorkingHoursAsync(reachedWorkingHours);
                }
            }
            catch (System.Exception)
            {
                await logService.SentryErrorAsync(JsonConvert.SerializeObject(reachedWorkingHours));
            }
        }
    }
}