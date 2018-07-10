namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface IConversationService
    {
        Task AddContact(Activity activity);
    }
}
