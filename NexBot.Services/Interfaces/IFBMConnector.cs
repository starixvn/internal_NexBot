namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models;

    public interface IFBMConnector
    {
        Task<string> Send(RegistVegetarianMealMessage registVegetarianMeal);
    }
}