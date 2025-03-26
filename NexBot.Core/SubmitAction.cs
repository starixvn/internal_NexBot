using System.ComponentModel;

namespace NexBot.Core
{
    public enum SubmitAction
    {
        ApproveRequest = 0,

        ProcessRequest = 1,

        RegistrationMeal = 2,

        [Description("Remind Registration Meal")]
        StopRemindRegistrationMeal = 3,

        [Description("Checkout Notification")]
        StopCheckOutNotification = 4,

        [Description("Reached Working Hours")]
        StopReachedWorkingHoursNotification = 5,

        [Description("Approve multiple requests")]
        ProcessMultipleRequests = 6,

        [Description("Checkin Notification")]
        StopCheckInNotification = 7,

        [Description("Start stop message category")]
        StartStopMessageCategory = 8,

        CommandMenu = 9,

        SubmitForm = 10,

        GetCheckInOutForm = 11,

        RegistryNewFingerMachineAlert = 12,

        RegistryBioChangeTimeAlert = 13,
    }
}