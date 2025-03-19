using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexBot.Models.Slack
{
    public class SlackInteractionPayload
    {
        [JsonPropertyName("actions")]
        public List<SlackAction> Actions { get; set; }

        [JsonPropertyName("team")]
        public SlackTeam Team { get; set; }

        [JsonPropertyName("channel")]
        public SlackChannel Channel { get; set; }

        [JsonPropertyName("message")]
        public SlackMessage Message { get; set; }

        [JsonPropertyName("state")]
        public SlackViewState State { get; set; }
    }


    public class SlackAction
    {
        [JsonPropertyName("action_id")]
        public string ActionId { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class SlackTeam
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class SlackChannel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class SlackMessage
    {
        [JsonPropertyName("bot_id")]
        public string BotId { get; set; }
    }

    public class SlackViewState
    {
        [JsonPropertyName("values")]
        public Dictionary<string, SlackActionElement> Values { get; set; }
    }

    public class SlackActionElement
    {
        [JsonPropertyName("data")]
        public SlackSelectedData Data { get; set; }
    }

    public class SlackSelectedData
    {
        [JsonPropertyName("selected_option")]
        public SlackSelectedOption SelectedOption { get; set; }

        [JsonPropertyName("selected_date")]
        public string SelectedDate { get; set; }

        [JsonPropertyName("selected_time")]
        public string SelectedTime { get; set; }
    }

    public class SlackSelectedOption
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
