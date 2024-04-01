namespace NexBot.Bots
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using NexBot.Bots.Interfaces;
    using System.Threading.Tasks;
    using System.Threading;

    public class CoreBot : ActivityHandler, ICoreBot
    {
        private const string greetingMessage = "Hello, I am {0}";

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, turnContext.Activity.Recipient.Name)), cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, turnContext.Activity.Recipient.Name)), cancellationToken);
        }
    }
}
