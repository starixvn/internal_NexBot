namespace NexBot.Models
{
    using System.Collections.Generic;

    public class BotConfigs
    {
        public int AppId { get; set; }

        public string MicrosoftAppType { get; set; }

        public string MicrosoftAppId { get; set; }

        public string MicrosoftAppPassword { get; set; }

        public string MicrosoftAppTenantId { get; set; }

        public IList<Channel> Channels { get; set; }
    }
}
