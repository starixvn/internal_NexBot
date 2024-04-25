namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]/[action]")]
    public class MessagesController(IMessageService messageService) : Controller
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
            if (record is not null)
            {
                await messageService.CheckoutNotificationAsync(record);
            }
        }
    }
}