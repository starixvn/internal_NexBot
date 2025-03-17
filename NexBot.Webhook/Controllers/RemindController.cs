namespace NexBot.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Collections.Generic;
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

        [HttpPost]

        public async Task<IActionResult> MissedLogTimeLeader(MissedLogTimeLeaderModel model)
        {
            model.Members = JsonConvert.DeserializeObject<IEnumerable<MemberModel>>(model.StringMembers);
            logService.LogFileMessage($"Remind log time leader: \nContent object: {JsonConvert.SerializeObject(model)}");
            var path = Path.Combine(".", "Resources", "RemindTemplates", "MissedLogTimeLeaderCard.json");
            var cardData = new Dictionary<string, object>
            {
                { "LeaderName", model.LeaderName },
                { "LogMonth", model.LogMonth }
            };
            var membersTextBlocks = new List<object>();
            if (model.Members != null)
            {
                foreach (var member in model.Members)
                {
                    membersTextBlocks.Add(new
                    {
                        type = "TextBlock",
                        text = $"   - {member.NickName} (**{member.MissedCount}** times)",
                        wrap = true,
                        spacing = "small"
                    });
                }
            }
            cardData.Add("MembersTextBlocks", membersTextBlocks);
            var attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel<object>(path, cardData);
            await messageService.SendAttachmentAsync(model.ConversationId, attachment);

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> MissedLogTimeMember(MissedLogTimeMemberModel model)
        {
            logService.LogFileMessage($"Remind log time member: \nContent object: {JsonConvert.SerializeObject(model)}");
            var path = Path.Combine(".", "Resources", "RemindTemplates", "MissedLogTimeMemberCard.json");
            var attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel<object>(path, model);
            await messageService.SendAttachmentAsync(model.ConversationId, attachment);
            return Ok();
        }
    }
}