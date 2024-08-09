namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public interface ITeamsBotHandler
    {
        Task HandleSubmitData(ITurnContext<IMessageActivity> turnContext);
    }
}