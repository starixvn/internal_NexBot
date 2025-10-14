namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Models;
    using NexBot.Models.Form;
    using NexBot.Models.Request;

    public interface IMessageService
    {
        Task SendAsync(string conversationId, string messageText, IList<Attachment> attachments = null);

        Task SendRequestAsync(RequestMessage requestMessage);

        Task RegistVegetarianMealAsync(RegistVegetarianMealMessage registVegetarianMessage);

        Task AddSystemLog(string logContent);

        Task ReSendErrorMeesage();

        Task CheckoutNotificationAsync(TimesheetRecord record);

        Task CheckinNotificationAsync(TimesheetRecord record);

        Task ReachedWorkingHoursAsync(ReachedWorkingHours reachedWorkingHours);

        Task MissingCheckInCheckOutAsync(CheckInCheckOutFormModel checkInCheckOutFormModel);

        Task MissingTimeLogMemberAsync(MissedLogTimeMemberModel model);

        Task MissingTimeLogLeaderAsync(MissedLogTimeLeaderModel model);

        Task RemindApproveMultiplePendingRequests(PendingRequest request);

        Task RemindCancelMealAsync(RemindCancelMealModel model);

        Task RemoveOldMessageLog();

        Task<Attachment> GetAttachmentHelpAdaptiveCard(string conversationId, bool isGroup, string botName);

        Task SendAttachmentAsync(string conversationId, Attachment attachment);

        Task SendBlockKitAsync(string conversationId, object block);

        Task<string> SendMessageFromHRToUserAsync(string fromHrUser, string messageContent, IAccountService accountService, IList<Attachment> attachments = null);

        Task SendMessageFromUserToHRAsync(string userName, string messageContent, string hrChannelId, IList<Attachment> attachments = null);
    }
}