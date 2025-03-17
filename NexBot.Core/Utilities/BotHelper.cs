namespace NexBot.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

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
                case Constants.SlackChannel:
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

        public static string Description(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = field.GetCustomAttributes(false);

            dynamic displayAttribute = null;

            if (attributes.Any())
            {
                displayAttribute = attributes.ElementAt(0);
            }

            return displayAttribute?.Description ?? string.Empty;
        }

        public static string FormatNickName(this string nickName)
        {
            var nameParts = nickName.Split(['.', ' '], StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < nameParts.Length; i++)
            {
                nameParts[i] = char.ToUpper(nameParts[i][0], CultureInfo.InvariantCulture) + nameParts[i][1..].ToLower(CultureInfo.InvariantCulture);
            }

            var processedNickName = string.Join(" ", nameParts);
            return processedNickName;
        }
    }
}