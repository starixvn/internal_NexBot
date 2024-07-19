using System.ComponentModel;

namespace NexBot.Core
{
    public enum SubmitAction
    {
        ProcessRequest = 1,
        RegistrationMeal = 2,

        [Description("Remind Registration Meal")]
        StopRemindRegistrationMeal = 3,

        [Description("Checkout Notification")]
        StopCheckOutNotification = 4,

        [Description("Reached Working Hours")]
        StopReachedWorkingHoursNotification = 5,

        [Description("Approve multiple requests")]
        ApproveMultipleRequests = 6,

        [Description("Checkin Notification")]
        StopCheckInNotification = 7,

        [Description("Start stop message category")]
        StartStopMessageCategory = 8,

        CommandMenu = 9
    }
}