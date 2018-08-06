namespace NexBot.Models.CategoryMessage
{
    public class CategoryMessage
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string RegisterCommand { get; set; }

        public string RemoveCommand { get; set; }

        public bool IsRegistered { get; set; }
    }
}