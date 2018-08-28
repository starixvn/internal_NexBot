namespace NexBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using NexBot.Core;
    using NexBot.Core.Services;
    using NexBot.Core.Utilities;
    using NexBot.Criteria.RegisterMessage;
    using NexBot.Models.CategoryMessage;
    using NexBot.Services;

    [Serializable]
    public class RegisterEventsDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            
            if (activity != null && activity.Text != null)
            {
                var message = GetStringInGroup(activity);
                if (message.Equals("help"))
                {
                    var options = BotHelper.MainEvents;

                    PromptDialog.Choice<string>(
                        context,
                        this.DisplaySelectedEvents,
                        options,
                        "What event would like to action?",
                        "Ooops, what you wrote is not a valid option, please try again",
                        3,
                        PromptStyle.Auto);
                }
                else if (message.Equals("test"))
                {
                    var subject = "Dear Quinn, \n\n"
+ "This is inform you about your member, Oliver(Đặng Hoàng Hải) had a leave request \n\n"
+ "- Date: 24 Aug, 2018 (0.5 day from 08:00 to 12:00) \n\n"
+ "- Type: Annual Leave \n\n"
+ "- Remark:  Tesing \n\n"
+ "Please process this request at: http://hrm.nexcel.vn/request/member \n\n"
           + "Any questions, please contact HR department.";

                    var cardActions = new List<CardAction>();

                    cardActions.Add(new CardAction(ActionTypes.PostBack, "Approve", value: "Approve"));
                    //cardActions.Add(new CardAction(ActionTypes.PostBack, "Reject", value: "Reject"));

                    var heroCard = new HeroCard
                    {
                        Buttons = cardActions
                    };

                    var attachment = heroCard.ToAttachment();

                    var messageReply = context.MakeMessage();
                    messageReply.Text = subject;
                    messageReply.Attachments.Add(attachment);

                    await context.PostAsync(messageReply);
                }
            }
        }

        public async Task DisplaySelectedEvents(IDialogContext context, IAwaitable<string> result)
        {
            var selectedMainEvent = await result;

            var message = context.MakeMessage();
            var conversationId = context.Activity.Conversation.Id;
            var attachment = await GetEventsCard(selectedMainEvent, conversationId);
            message.Attachments.Add(attachment);

            await context.PostAsync(message);

            context.Wait(this.MessageReceivedAsync);
        }

        private async Task<Attachment> GetEventsCard(string selectedCard, string conversationId)
        {
            switch (selectedCard)
            {
                case Constants.RegisterEvents:
                    return await GetRegisterEvents(conversationId);
                case Constants.RemoveEvents:
                    return await GetRemoveEvents(conversationId);

                default:
                    return await GetRemoveEvents(conversationId);
            }
        }

        private async Task<Attachment> GetRegisterEvents(string conversationId)
        {
            var categories = await GetNoneRegisteredCategory(conversationId);
            var cardActions = new List<CardAction>();
            foreach (var category in categories)
            {
                var cardAction = new CardAction(ActionTypes.PostBack, category.Name, value: category.RegisterCommand);
                cardActions.Add(cardAction);
            }

            var heroCard = new HeroCard
            {
                Subtitle = "Please register the event!",
                Images = new List<CardImage> { new CardImage("http://stokvelex.co.za/wp-content/themes/StokvelEx/images/regi-banner.png") },
                Buttons = cardActions
            };

            return heroCard.ToAttachment();
        }

        private async Task<IList<CategoryMessage>> GetRegisteredCategory(string conversationId)
        {
            var service = new RegisterMessageService();

            var categories = await service.GetAllCategoryMessage(conversationId);
            return categories.Where(t=>t.IsRegistered).ToList();
        }

        private async Task<IList<CategoryMessage>> GetNoneRegisteredCategory(string conversationId)
        {
            var service = new RegisterMessageService();
            var categories = await service.GetAllCategoryMessage(conversationId);
            return categories.Where(t => !t.IsRegistered).ToList();
        }

        private async Task<Attachment> GetRemoveEvents(string conversationId)
        {
            var categories = await GetRegisteredCategory(conversationId);
            var cardActions = new List<CardAction>();
            foreach (var category in categories)
            {
                var cardAction = new CardAction(ActionTypes.PostBack, category.Name, value: category.RemoveCommand);
                cardActions.Add(cardAction);
            }

            var heroCard = new HeroCard
            {
                Subtitle = "Please remove the event!",
                Images = new List<CardImage> { new CardImage("https://www.thierrybisch.com/Images/STUDIO/Fondation%20Prince/LOGO%20Delete%20PM.png") },
                Buttons = cardActions
            };

            return heroCard.ToAttachment();
        }

        private string GetStringInGroup(IMessageActivity activity)
        {
            var message = activity.Text;

            if (activity.Conversation.IsGroup == true)
            {
                message = BotHelper.GetMessageInGroupByChannel(message, activity.ChannelId, activity.Recipient.Name);
            }

            return message;
        }
    }
}