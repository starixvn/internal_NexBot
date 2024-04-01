namespace NexBot.Bots.Interfaces
{
    using Microsoft.Bot.Builder;
    using System.Threading.Tasks;
    using System.Threading;

    public interface IChannelBot
    {
        Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken));
    }
}
