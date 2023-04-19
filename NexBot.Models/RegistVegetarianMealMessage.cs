namespace NexBot.Models
{
    using System;

    public class RegistVegetarianMealMessage
    {
        public string ConversationId { get; set; }

        public int EmpId { get; set; }

        public int CompanyId { get; set; }

        public DateTime OnDate { get; set; }

        public string Message { get; set; }

        public string CallBackUrl { get; set; }
    }
}