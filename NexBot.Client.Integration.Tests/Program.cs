using System;

namespace NexBot.Client.Integration.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var botConnector = new BotConnector();
            var result = botConnector.Send(
                "Good morning !!", "29:1B0ITrMh54YOyDr7OHDCwULK81Ok3QQcOBrQYrVlw9vo");
            //var result = botConnector.Send(
            //   "Đi ngủ thôi nào", "245351906");
            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }
}
