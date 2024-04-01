using System.Collections.Generic;

namespace NexBot.Core.Utilities
{
    public static class BotHelper
    {
        public static IList<string> MainEvents
        {
            get
            {
                return new List<string>
                {
                    Constants.RegisterEvents,
                    Constants.RemoveEvents
                };
            }
        }

        public static string GenerateMessage(string message)
        {
            var returnMessage = message;

            if (message is not null && message.StartsWith('@'))
            {
                var indexOfCommand = message.IndexOf(' ');

                if (indexOfCommand > 0)
                {
                    returnMessage = message.Remove(0, indexOfCommand).Trim();
                }
            }

            return returnMessage;
        }

        public static string GetMessageInGroupByChannel(string sourceMessage, string channelId, string botName)
        {
            var returnMessage = sourceMessage;

            switch (channelId)
            {
                case Constants.SkypeChannel:
                    if (sourceMessage.StartsWith('@') 
                        || sourceMessage.StartsWith(botName))
                    {
                        var indexOfCommand = sourceMessage.IndexOf(' ');

                        if (indexOfCommand > 0)
                        {
                            returnMessage = sourceMessage.Remove(0, indexOfCommand).Trim();
                        }
                    }
                    break;
                case Constants.TelegramChannel:
                    if (sourceMessage.StartsWith('/'))
                    {
                        var indexOfCommand = sourceMessage.IndexOf(' ');

                        if (indexOfCommand > 0)
                        {
                            returnMessage = sourceMessage.Remove(0, indexOfCommand).Trim();
                        }
                    }
                    break;
                case Constants.TeamsChannel:
                    if (sourceMessage.StartsWith("<at>"))
                    {
                        var indexOfCommand = sourceMessage.IndexOf("</at>");

                        if (indexOfCommand > 0)
                        {
                            returnMessage = sourceMessage.Remove(0, indexOfCommand + 5).Trim();
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