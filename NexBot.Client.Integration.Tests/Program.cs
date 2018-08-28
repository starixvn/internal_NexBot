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
+ "- Date: 24 Aug, 2018(0.5 day from 08:00 to 12:00) \n\n"
+ "- Type: Annual Leave \n\n"
+ "- Remark:  Tesing \n\n"
+ "Please process this request at: http://hrm.nexcel.vn/request/member \n\n"
            + "Any questions, please contact HR department.";

            var botConnector = new BotConnector();

            var cardActions = new List<CardAction>();
            
            cardActions.Add(new CardAction(ActionTypes.PostBack, "Approve", value: "Approve"));
            cardActions.Add(new CardAction(ActionTypes.PostBack, "Reject", value: "Reject"));

            var heroCard = new HeroCard
            {
                Title = subject,
                
                Buttons = cardActions
            };

            var attachment = heroCard.ToAttachment();

            //var result = botConnector.Send(
            //    "Good morning !!", "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo");

            var result1 = botConnector.Send1(
                subject, "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo", attachment);

            

            System.Console.WriteLine(result1);
            System.Console.ReadLine();
        }
    }
}