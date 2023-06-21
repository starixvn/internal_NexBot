namespace NexBot.Bots
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface ISkypeBot
    {
        Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken);

        Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);

        Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken);

        Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken);

    }
}
