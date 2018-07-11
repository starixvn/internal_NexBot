namespace NexBot.Core.Utilities
{
    public static class BotHelper
    {
        public static string GenerateMessage(string message)
        {
            var returnMessage = message;

            if (message.StartsWith("@"))
            {
                var indexOfCommand = message.IndexOf(' ');

                if (indexOfCommand > 0)
                {
                    returnMessage = message.Remove(0, indexOfCommand).Trim();
                }
            }

            return returnMessage;
        }
    }
}