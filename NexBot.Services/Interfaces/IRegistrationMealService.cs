namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IRegistrationMealService
    {
        Task RegistVegetarian(string conversationId, string commandRequest);
    }
}
