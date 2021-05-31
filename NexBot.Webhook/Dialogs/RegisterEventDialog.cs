namespace NexBot.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;
    using NexBot.Core;
    using NexBot.Services.Interfaces;

    public class RegisterEventDialog : ComponentDialog
    {
        private readonly DialogSet dialogSet;
        private readonly IRegisterMessageService registerMessageService;
        private readonly string DlgMainId = "MainDialog";
        private readonly string DlgHelperId = "DlgHelperId";
        private readonly string DlgEventId = "DlgEventId";
        private readonly string DlgEventSelectedId = "DlgEventSelectedId";

        public RegisterEventDialog(IRegisterMessageService registerMessageService, string dialogId = null) : base(dialogId)
        {
            this.registerMessageService = registerMessageService;

            dialogSet.Add(new ChoicePrompt(DlgHelperId, ShowHelperDialogCard));
            dialogSet.Add(new ChoicePrompt(DlgEventId, ShowEventDialogCard));
            dialogSet.Add(new ChoicePrompt(DlgEventSelectedId, ShowSelectedEventDialogCard));

            var waterfallSteps = new WaterfallStep[]
           {
                SelectHelperAsync,
                SelectEventAsync
           };

            dialogSet.Add(new WaterfallDialog(DlgMainId, waterfallSteps));
        }

        private Task<bool> ShowHelperDialogCard(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private Task<bool> ShowEventDialogCard(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private Task<bool> ShowSelectedEventDialogCard(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private async Task<DialogTurnResult> SelectHelperAsync(WaterfallStepContext stepContext,
           CancellationToken cancellationToken)
        {
            var options = new List<string> { Constants.RegisterEvents, Constants.RemoveEvents };

            return await stepContext.PromptAsync(DlgHelperId, new PromptOptions()
            {
                Prompt = MessageFactory.Text("What event would like to action?"),
                Choices = ChoiceFactory.ToChoices(options),
                RetryPrompt = MessageFactory.Text("Ooops, what you wrote is not a valid option, please try again"),
                Style = ListStyle.HeroCard
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectEventAsync(WaterfallStepContext stepContext,
          CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            bool isGetRegisteredEvents = choice.Value.Equals(Constants.RemoveEvents);
            var options = await GetRegisterEvents(stepContext.Context.Activity.Conversation.Id, isGetRegisteredEvents);
            var actionName = isGetRegisteredEvents ? "remove" : "register";
            return await stepContext.PromptAsync(DlgEventId, new PromptOptions()
            {
                Prompt = MessageFactory.Text($"What event would like to {actionName}?"),
                Choices = ChoiceFactory.ToChoices(options),
                RetryPrompt = MessageFactory.Text("Ooops, what you wrote is not a valid option, please try again"),
                Style = ListStyle.HeroCard
            }, cancellationToken);
        }

        private async Task<List<string>> GetRegisterEvents(string conversationId, bool isRegistered)
        {
            var categories = await registerMessageService.GetAllCategoryMessage(conversationId);
            return categories.Where(t => t.IsRegistered == isRegistered).Select(e => e.Name).ToList();
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                DialogContext dlgContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken: cancellationToken);

                if (dlgContext != null && dlgContext.ActiveDialog is null)
                {
                    await dlgContext.BeginDialogAsync(DlgMainId, null, cancellationToken);
                }
                else if (dlgContext != null && dlgContext.ActiveDialog != null)
                {
                    await dlgContext.ContinueDialogAsync(cancellationToken);
                }
            }
        }
    }
}