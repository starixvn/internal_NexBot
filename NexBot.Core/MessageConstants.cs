using System.Text;

namespace NexBot.Core
{
    public static class MessageConstants
    {
        public static string HelpCommand(string botName, bool isGroup)
        {
            var strBuilder = new StringBuilder($"**{botName}**'s available commands:");
            strBuilder.AppendLine();
            strBuilder.AppendLine("- **getid** => Get conversation ID");
            strBuilder.AppendLine("- **getbot** => Get bot conversation ID");
            if (isGroup)
            {
                strBuilder.AppendLine("- **regist** => Register this conversation");
            }
            else
            {
                strBuilder.AppendLine("- **stop_remind_vegetarian_meal** => Stop receive message remind register vegatarian meal. Useful when you never vegetarian");
                strBuilder.AppendLine("- **start_remind_vegetarian_meal** => Start receive message remind register vegatarian meal.");
                strBuilder.AppendLine("- **stop_remind_reachedworkingtime** => Stop receive message remind you are reached the working hours on the day.");
                strBuilder.AppendLine("- **start_remind_reachedworkingtime** => Start receive message remind you are reached the working hours on the day.");
            }

            return strBuilder.ToString();
        }
    }
}