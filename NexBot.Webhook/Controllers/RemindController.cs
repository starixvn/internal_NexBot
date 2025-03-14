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
    using System;

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

        [HttpPost]
        public async Task<IActionResult> SendForm()
        {
            var path = Path.Combine(".", "Resources", "SlackBlockKit", "LeaveRequestCard.json");
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(path, new
            {
                AvatarUrl = "https://hrm.nexdev.net/Employee/Avatar/02751",
                CreatedDate = "2021-09-09",
                RequestDate = "2021-09-09",
                Time = "09:00",
                RequestId = 1,
                Nick = "Henry",
                FullName = "Dinh Quang Hoang",
                Supervisor = "Julien",
                RequestType = "Check In",
                Shift = "M1"
            });
            await messageService.SendBlockKitAsync("B08HJT3E2D8:T08ACMHMMFS:D08HBDU0UQ6", block);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendCheckInForm()
        {
            var path = Path.Combine(".", "Resources", "SlackBlockKit", "CheckInCheckOutFormCard.json");
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(path, new CheckInCheckOutFormModel
            {
                EmpId = 1234,
                NickName = "Henry",
                MissingType = 1,
                Shift = "M1 (08:00 > 18:00)",
                AttendanceDate = DateTime.UtcNow,
                ConversationId = "B08HJT3E2D8:T08ACMHMMFS:D08HBDU0UQ6"
            });
            await messageService.SendBlockKitAsync("B08HJT3E2D8:T08ACMHMMFS:D08HBDU0UQ6", block);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendMissingForm()
        {
            var path = Path.Combine(".", "Resources", "SlackBlockKit", "MissingCheckInOutCard.json");
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(path, new 
            {
                NickName = "Henry",
                MissingTypeText = "Check In",
                DisplayDateString = "10 Mar, 2024",
                Shift = "M1 (08:00 > 18:00)",
            });
            await messageService.SendBlockKitAsync("B08HJT3E2D8:T08ACMHMMFS:D08HBDU0UQ6", block);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendCheckinoutForm()
        {
            var path = Path.Combine(".", "Resources", "SlackBlockKit", "NotifyCheckoutCard.json");
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(path, new
            {
                NickName = "John",
                CheckoutTime = "08:09 AM"
            });
            await messageService.SendBlockKitAsync("B08HJT3E2D8:T08ACMHMMFS:D08HBDU0UQ6", block);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendReachedForm()
        {
            var path = Path.Combine(".", "Resources", "SlackBlockKit", "ReachedWorkingHoursCard.json");
            var block = SlackBlockKitHelper.GenerateSlackBlockKit(path, new
            {
                NickName = "John",
                WorkingHours = "08:00'"
            });
            await messageService.SendBlockKitAsync("B08HJT3E2D8:T08ACMHMMFS:D08HBDU0UQ6", block);
            return Ok();
        }
    }
}