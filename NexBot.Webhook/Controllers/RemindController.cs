namespace NexBot.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models.Form;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]/[action]")]
    public class RemindController(IMessageService messageService, ILogProviderService logService) : Controller
    {
        [HttpPost]
        public async Task MissingCheckInCheckOut(CheckInCheckOutFormModel checkInCheckOutFormModel)
        {
            if (checkInCheckOutFormModel is not null)
            {
                await messageService.MissingCheckInCheckOutAsync(checkInCheckOutFormModel);
            }
        }

        [HttpPost]

        public async Task MissedLogTimeLeader(MissedLogTimeLeaderModel model)
        {
            if (model is not null)
            {
                await messageService.MissingTimeLogLeaderAsync(model);
            }
        }


        [HttpPost]
        public async Task MissedLogTimeMember(MissedLogTimeMemberModel model)
        {
            if (model is not null)
            {
                await messageService.MissingTimeLogMemberAsync(model);
            }
        }
    }
}