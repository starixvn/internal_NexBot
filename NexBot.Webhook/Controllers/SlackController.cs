namespace NexBot.Controllers
{
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.AspNetCore.Mvc;
    using NexBot.Models.Slack;
    using NexBot.Services.Interfaces;

    [Route("api/slack/actions")]
    [ApiController]
    public class SlackController(ISlackBotHandler slackBotHandler) : ControllerBase
    {
        [HttpPost]
        public async Task PostAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var rawPayload = await reader.ReadToEndAsync();
            var parsedData = HttpUtility.ParseQueryString(rawPayload);
            var payloadJson = parsedData["payload"];

            if (string.IsNullOrEmpty(payloadJson))
            {
                return;
            }

            var payload = JsonSerializer.Deserialize<SlackInteractionPayload>(payloadJson);
            await slackBotHandler.HandleInteractiveRequest(payload);
        }
    }
}