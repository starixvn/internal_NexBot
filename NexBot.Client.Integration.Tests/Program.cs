namespace NexBot.Client.Integration.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var botConnector = new BotConnector();
            var result = botConnector.Send(
                "Good morning !!", "19:6d77b80f873f46679c2c39396220fba6@thread.skype");
            //var result = botConnector.Send(
            //   "Đi ngủ thôi nào", "245351906");
            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }
}