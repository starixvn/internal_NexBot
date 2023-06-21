namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]/[action]")]
    public class MessagesController : Controller
    {
        private readonly IMessageService messageService;

        public MessagesController(IMessageService messageService)
        {
            this.messageService = messageService;
        }

        //[Authorize(Roles = "Bot")]
        [HttpPost]
        //[Route("Forward")]
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
            ////var registVegetarianCommand = string.Format("{0}_{1}_{2}_{3}_{4}", "cmdRegistVegetarianMeal", model.EmpId, model.CompanyId, model.OnDate.ToString("yyyy-MM-dd"), model.CallBackUrl);
            ////var result = await registrationMealService.RegistVegetarian(model.ConversationId, registVegetarianCommand);
            await messageService.RegistVegetarianMealAsync(model);
        }
    }
}