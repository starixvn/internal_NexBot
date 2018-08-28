using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace NexBot.Client.Integration.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {



            var botConnector = new BotConnector();

            var cardActions = new List<CardAction>();
            
            cardActions.Add(new CardAction(ActionTypes.PostBack, "Approve", value: "Approve"));
            cardActions.Add(new CardAction(ActionTypes.PostBack, "Reject", value: "Reject"));

            var heroCard = new HeroCard
            {
                Subtitle = "Please Action request!",
                Buttons = cardActions
            };

            var attachment = heroCard.ToAttachment();

            var result = botConnector.Send(
                "Good morning !!", "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo");

            var result1 = botConnector.Send1(
                "Good morning !!", "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo", attachment);

            

            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }
}