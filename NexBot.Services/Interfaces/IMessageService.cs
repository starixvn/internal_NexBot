namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models;

    public interface IMessageService
    {
        Task SendAsync(string conversationId, string messageText);

        Task SendRequestAsync(SendRequestMessage requestMessage);

        Task RegistVegetarianMealAsync(RegistVegetarianMealMessage message);

        Task AddSystemLog(string logContent);

        Task ReSendErrorMeesage();
    }
}