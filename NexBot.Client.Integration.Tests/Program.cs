using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace NexBot.Client.Integration.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var subject = "Dear Quinn, \n\n"
+ "This is inform you about your member, Oliver(Đặng Hoàng Hải) had a leave request \n\n"
+ "_- Date: 24 Aug, 2018 (0.5 day from 08:00 to 12:00)_ \n\n"
+ "_- Type: Annual Leave_ \n\n"
+ "_- Remark:  Tesing1 _ \n\n"
+ "Please process this request at: http://hrm.nexcel.vn/request/member \n\n"
            + "Any questions, please contact HR department.";

            var botConnector = new BotConnector();

            var result1 = botConnector.Send(
                subject, "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo");

            

            System.Console.WriteLine(result1);
            System.Console.ReadLine();
        }
    }
}