namespace NexBot.Bots
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using NexBot.Core;

    public class BotHandler : ActivityHandler
    {
        private readonly ISkypeBot skypeBot;
        private readonly ITelegramBot telegramBot;

        private const string greetingMessage = "Hello, I am {0}";

        public BotHandler(ISkypeBot skypeBot,
            ITelegramBot telegramBot)
        {
            this.skypeBot = skypeBot;
            this.telegramBot = telegramBot;
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.ChannelId)
            {
                case Constants.SkypeChannel:
                    await skypeBot.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
                    break;
                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, turnContext.Activity.Recipient.Name)), cancellationToken);
                    break;
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.ChannelId)
            {
                case Constants.SkypeChannel:
                    await skypeBot.OnMessageActivityAsync(turnContext, cancellationToken);
                    break;

                case Constants.TelegramChannel:
                    await telegramBot.OnMessageActivityAsync(turnContext, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, turnContext.Activity.Recipient.Name)), cancellationToken);
                    break;
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.ChannelId)
            {
                case Constants.SkypeChannel:
                    await skypeBot.OnEventActivityAsync(turnContext, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, turnContext.Activity.Recipient.Name)), cancellationToken);
                    break;
            }
        }

        protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.ChannelId)
            {
                case Constants.SkypeChannel:
                    await skypeBot.OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(greetingMessage, turnContext.Activity.Recipient.Name)), cancellationToken);
                    break;
            }
        }
    }
}