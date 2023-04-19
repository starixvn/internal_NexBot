namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Core.Models;
    using NexBot.Models;

    public interface IFBMConnector
    {
        Task<CommandResult> Send(RegistVegetarianMealMessage registVegetarianMeal);
    }
}