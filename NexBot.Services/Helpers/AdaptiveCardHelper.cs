namespace NexBot.Services.Helpers
{
    using System.IO;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    public static class AdaptiveCardHelper
    {
        public static Attachment GenerateAdaptiveCardTeamsChannel<T>(string templatePath, T model)
        {
            var templateJson = File.ReadAllText(templatePath);
            AdaptiveCardTemplate template = new(templateJson);
            string cardJson = template.Expand(model);

            var cardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            return cardAttachment;
        }
    }
}