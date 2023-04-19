namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IRegistrationMealService
    {
        Task<string> RegistVegetarian(string conversationId, string commandRequest);
    }
}
