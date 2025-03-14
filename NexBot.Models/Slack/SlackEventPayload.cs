using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexBot.Models.Slack
{
    public class SlackEventPayload
    {
        public SlackEventMessage SlackMessage { get; set; }

        public string ApiToken { get; set; }
    }

    public class SlackEventMessage
    {
        public SlackEvent Event { get; set; }
    }

    public class SlackEvent
    {
        public string Type { get; set; }

        public string User { get; set; }
    }
}
