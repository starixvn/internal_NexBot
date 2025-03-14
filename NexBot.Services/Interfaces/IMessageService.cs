namespace NexBot.Services.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Models;
    using NexBot.Models.Request;

    public interface IMessageService
    {
        Task SendAsync(string conversationId, string messageText);

        Task SendRequestAsync(RequestMessage requestMessage);

        Task RegistVegetarianMealAsync(RegistVegetarianMealMessage registVegetarianMessage);

        Task AddSystemLog(string logContent);

        Task ReSendErrorMeesage();

        Task CheckoutNotificationAsync(TimesheetRecord record);

        Task CheckinNotificationAsync(TimesheetRecord record);

        Task ReachedWorkingHoursAsync(ReachedWorkingHours reachedWorkingHours);

        Task RemindApproveMultiplePendingRequests(PendingRequest request);

        Task RemoveOldMessageLog();

        Task<Attachment> GetAttachmentHelpAdaptiveCard(string conversationId, bool isGroup, string botName);

        Task SendAttachmentAsync(string conversationId, Attachment attachment);

        Task SendBlockKitAsync(string conversationId, object block);
    }
}