namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;

    public interface IFormAdaptiveCardService
    {
        Task<Attachment> GetCheckInCheckOutAdaptiveCardForm(string botConversationId, string conversationId);

        Task<object> GetCheckInCheckOutBlockKitForm(string botConversationId, string callbackUrl, string conversationId);
    }
}