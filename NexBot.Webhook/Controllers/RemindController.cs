namespace NexBot.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models.Form;
    using NexBot.Services.Helpers;
    using NexBot.Services.Interfaces;

    [Route("api/[controller]/[action]")]
    public class RemindController(IMessageService messageService, ILogProviderService logService) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> MissingCheckInCheckOut(CheckInCheckOutFormModel checkInCheckOutFormModel)
        {
            logService.LogFileMessage($"Remind check in/check out: \nContent object: {JsonConvert.SerializeObject(checkInCheckOutFormModel)}");
            var path = Path.Combine(".", "Resources", "RemindTemplates", "MissingCheckInOutCard.json");
            var attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel<object>(path, checkInCheckOutFormModel);
            await messageService.SendAttachmentAsync(checkInCheckOutFormModel.ConversationId, attachment);
            return Ok();
        }
    }
}