namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using NexBot.Models;
    using NexBot.Models.Request;

    public interface IMessageService
    {
        Task SendAsync(string conversationId, string messageText);

        Task SendRequestAsync(RequestMessage requestMessage);

        Task RegistVegetarianMealAsync(RegistVegetarianMealMessage registVegetarianMessage);

        Task AddSystemLog(string logContent);

        Task ReSendErrorMeesage();

        Task CheckoutNotificationAsync(CheckoutRecord record);

        Task ReachedWorkingHoursAsync(ReachedWorkingHours reachedWorkingHours);

        Task RemindApproveMultiplePendingRequests(PendingRequest request);
    }
}