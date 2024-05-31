namespace NexBot
{
    using System;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NexBot.Core.Services.Interfaces;

    public class AdapterWithErrorHandler : CloudAdapter
    {
        public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<IBotFrameworkHttpAdapter> logger, ILogProviderService logService, ConversationState conversationState = default)
            : base(auth, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                // NOTE: In production environment, you should consider logging this to
                // Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
                // to add telemetry capture to your bot.
                await logService.SentryErrorAsync($"[OnTurnError] unhandled error : {exception.Message}. " +
                     $"Type: {turnContext.Activity?.Type}. " +
                    $"Channel: {turnContext.Activity?.ChannelId}. " +
                    $"ChannelData: {JsonConvert.SerializeObject(turnContext.Activity?.ChannelData)}. " +
                    $"Message: {JsonConvert.SerializeObject(turnContext.Activity?.Text)}. " +
                    $"Action: {turnContext.Activity?.Action}. " +
                    $"From: {turnContext.Activity.From?.Id}, name: {turnContext.Activity.From?.Name}. " +
                    $"Conversation: {turnContext.Activity.Conversation?.Id}, name: {turnContext.Activity.Conversation?.Name}. " +
                    $"To: {turnContext.Activity.Recipient?.Id}, name: {turnContext.Activity.Recipient?.Name}"
                    , exception);

                // Send a message to the user
                //var errorMessageText = "The bot encountered an error or bug. To continue to run this bot, please fix the bot source code.";
                //var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
                //await turnContext.SendActivityAsync(errorMessage);
                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        await logService.SentryErrorAsync($"Exception caught on attempting to Delete ConversationState : {e.Message}", e);
                    }
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                //await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}