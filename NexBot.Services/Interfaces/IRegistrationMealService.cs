namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models;

    public interface IRegistrationMealService
    {
        Task<string> RegistVegetarian(string conversationId, string commandRequest);

        Task<string> RegistVegetarian(RegistVegetarianMealMessage registVegetarianMealMessage);

        Task<string> CancelMeal(CancelMealFormData cancelMealFormData);
    }
}
