namespace NexBot.Services
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Services.Helpers;
    using NexBot.Services.Interfaces;

    public class FormAdaptiveCardService(IHRMConnector hrm, IApplicationSettingService applicationSetting) : IFormAdaptiveCardService
    {
        public async Task<Attachment> GetCheckInCheckOutAdaptiveCardForm(string botConversationId, string conversationId)
        {
            Attachment attachment = null;
            var url = applicationSetting.GetHrmUrl(botConversationId);
            if (!string.IsNullOrEmpty(url))
            {
                var checkInCheckOutModel = await hrm.GetCheckInCheckOutModel(url, conversationId);
                if (checkInCheckOutModel != null)
                {
                    attachment = AdaptiveCardHelper.GenerateAdaptiveCardTeamsChannel(
                         Path.Combine(".", "Resources", "Requests", "CheckInCheckOutFormCard.json"),
                         checkInCheckOutModel);
                }
            }
            return attachment;
        }

        public async Task<object> GetCheckInCheckOutBlockKitForm(string botConversationId, string callbackUrl, string conversationId)
        {
            object block = null;
            var url = callbackUrl ?? applicationSetting.GetHrmUrl(botConversationId);
            if (!string.IsNullOrEmpty(url))
            {
                var checkInCheckOutModel = await hrm.GetCheckInCheckOutModel(url, conversationId);
                if (checkInCheckOutModel != null)
                {
                    checkInCheckOutModel.CallbackUrl = callbackUrl;
                    block = SlackBlockKitHelper.GenerateSlackBlockKit(
                        Path.Combine(".", "Resources", "SlackBlockKit", "CheckInCheckOutFormCard.json"),
                        checkInCheckOutModel);
                }
            }
            return block;
        }
    }
}