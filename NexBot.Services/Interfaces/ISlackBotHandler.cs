namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models.Slack;

    public interface ISlackBotHandler
    {
        Task HandleInteractiveRequest(SlackInteractionPayload payload);

        Task PublishAppHomeView(SlackAppHomeViewPayload payload);
    }
}