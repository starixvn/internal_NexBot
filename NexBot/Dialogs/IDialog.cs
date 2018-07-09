namespace NexBot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Utilitites;

    public interface IDialog
    {
        IConversation Conversation { get; }

        Task RegisterMessageInfo(IMessageActivity activity);
    }
}