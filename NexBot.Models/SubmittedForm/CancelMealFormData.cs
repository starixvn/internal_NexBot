namespace NexBot.Models
{
    using System;

    public class CancelMealFormData
    {
        public int SysId { get; set; }

        public DateTime OnDate { get; set; }

        public string CallBackUrl { get; set; }

        public int MealId { get; set; }
    }
}