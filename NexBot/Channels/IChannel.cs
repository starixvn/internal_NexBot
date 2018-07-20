namespace NexBot.Channels
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;

    public interface IChannel
    {
        Task Post(Activity activity);
    }
}
