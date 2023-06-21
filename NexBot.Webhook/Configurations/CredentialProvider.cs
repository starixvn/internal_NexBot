namespace NexBot.Configurations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.Authentication;

    public class CredentialProvider : ICredentialProvider
    {
        private readonly Dictionary<string, string> Credentials;

        public CredentialProvider(BotConfigurations botConfig)
        {
            Credentials = new Dictionary<string, string> {
                {
                    botConfig.MicrosoftAppId, botConfig.MicrosoftAppPassword
                }
            };
        }

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(Credentials.ContainsKey(appId));
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult(Credentials.ContainsKey(appId) ? Credentials[appId] : null);
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(!Credentials.Any());
        }
    }
}