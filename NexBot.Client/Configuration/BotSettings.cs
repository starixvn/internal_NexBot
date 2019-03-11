namespace NexBot.Client.Configuration
{
    using System;
    using System.Configuration;

    public class BotSettings
    {
        public static BotSettings Settings => new BotSettings();

        public static Uri BotServiceUrl { get; internal set; }
            = new Uri(ConfigurationManager.AppSettings["FanexBotClient:BotServiceUrl"] ??
                "https://nexbot.tk/api/");

        public static string ClientId { get; internal set; }
            = ConfigurationManager.AppSettings["MicrosoftAppId"] ??
                "1fca3d04-4799-4544-8744-89367a838238";

        public static string ClientPassword { get; internal set; }
            = ConfigurationManager.AppSettings["MicrosoftAppPassword"] ??
                "NJT594-!uoobpdkYYBF29:^";

        public static Uri TokenUrl { get; internal set; }
            = new Uri(ConfigurationManager.AppSettings["TokenUrl"] ??
                "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token");
    }
}