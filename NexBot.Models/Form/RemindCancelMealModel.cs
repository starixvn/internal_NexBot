using System;

namespace NexBot.Models.Form
{
    public class RemindCancelMealModel
    {
        public string ConversationId { get; set; }
        public string NickName { get; set; }
        public int SysId { get; set; }
        public int MealId { get; set; }
        public DateTime OnDate { get; set; }
        public string CallBackUrl { get; set; }
        public string DisplayDateString => OnDate.ToString("dd/MM/yyyy");
    }
}