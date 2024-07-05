namespace NexBot.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Models.Request;
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
        public async Task CheckoutNotification(TimesheetRecord record)
        {
            if (record is not null)
            {
                await messageService.CheckoutNotificationAsync(record);
            }
        }

        [HttpPost]
        public async Task CheckinNotification(TimesheetRecord record)
        {
            if (record is not null)
            {
                await messageService.CheckinNotificationAsync(record);
            }
        }

        [HttpPost]
        public async Task ReachedWorkingHoursNotification(ReachedWorkingHours reachedWorkingHours)
        {
            if (reachedWorkingHours is not null)
            {
                await messageService.ReachedWorkingHoursAsync(reachedWorkingHours);
            }
        }

        [HttpPost]
        public async Task RemindApproveMultipleRequest(PendingRequest request)
        {
            if (request is not null)
            {
                request.Requests = JsonConvert.DeserializeObject<IEnumerable<MemberRequest>>(request.StringRequests);
                logService.LogFileMessage(JsonConvert.SerializeObject(request));

                await messageService.RemindApproveMultiplePendingRequests(request);
            }
        }
    }
}