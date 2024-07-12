namespace NexBot.Models.CategoryMessage
{
    public class MessageCategorySubmitData
    {
        public int CategoryId { get; set; }
        public string Command { get; set; }
        public bool IsNeedRegister { get; set; }
    }
}