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

        public static string GetMessageInGroupByChannel(string sourceMessage, string channelId)
        {
            var returnMessage = string.Empty;

            switch (channelId)
            {
                case "skype":
                    if (sourceMessage.StartsWith("@"))
                    {
                        var indexOfCommand = sourceMessage.IndexOf(' ');

                        if (indexOfCommand > 0)
                        {
                            returnMessage = sourceMessage.Remove(0, indexOfCommand).Trim();
                        }
                    }
                    break;
                case "telegram":
                    if (sourceMessage.StartsWith("/"))
                    {
                        var indexOfCommand = sourceMessage.IndexOf(' ');

                        if (indexOfCommand > 0)
                        {
                            returnMessage = sourceMessage.Remove(0, indexOfCommand).Trim();
                        }
                    }
                    break;
                default:
                    returnMessage = sourceMessage;
                    break;
            }

            return returnMessage;
        }
    }
}