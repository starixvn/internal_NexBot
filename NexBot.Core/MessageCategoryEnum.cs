namespace NexBot.Core
{
    using System.ComponentModel;

    public enum MessageCategoryEnum
    {
        [Description("New Finger Machine Alert")]
        NewFingerMachineAlert = 1,

        [Description("BIO Change Time Alert")]
        BIOChangeTimeAlert = 2,

        [Description("New Finger Print & Card Alert")]
        NewFingerPrintAndCardAlert = 3,

        [Description("Deduct AL when exceeding Late In/ Early Out Alert")]
        DeductAnnualBalanceAlert = 4,

        [Description("Reminder Forgot spend time")]
        ReminderForgoSpendTime = 5,

        [Description("Reminder Missing Check In/Out")]
        ReminderMissingCheckInOut = 6,

        [Description("Warning Violate Late In/ Early Out In a Month")]
        WarningViolateLateInEarlyOut = 7,

        [Description("Warning Not Follow Late In/ Early Out Policy")]
        WarningNotFollowLateInEarlyOut = 8,

        [Description("Reminder Reached Working Hour")]
        ReminderReachedWorkingHour = 9
    }
}
