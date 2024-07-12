namespace NexBot.Models.CategoryMessage
{
    using System.Collections.Generic;

    public class HelpMenuAdaptiveCard
    {
        public string BotName { get; set; }
        public IEnumerable<RegisterCategoryMessage> RegisterCategoryMessages { get; set; }
    }

    public class RegisterCategoryMessage
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsNeedRegister { get; set; }
    }
}