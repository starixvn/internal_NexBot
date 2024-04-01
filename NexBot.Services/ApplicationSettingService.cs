namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class ApplicationSettingService : IApplicationSettingService
    {
        private readonly IList<BotConfigs> botConfigs;

        public ApplicationSettingService(IList<BotConfigs> botConfigs)
        {
            this.botConfigs = botConfigs;
        }

        public async Task<IEnumerable<ApplicationSetting>> GetApplicationSettingAsync()
        {
            var applicationSettings = new List<ApplicationSetting>();
            foreach (var botConfig in botConfigs)
            {
                applicationSettings.Add(new ApplicationSetting
                {
                    AppId = botConfig.AppId,
                    MicrosoftAppType = botConfig.MicrosoftAppType,
                    MicrosoftAppId = botConfig.MicrosoftAppId,
                    MicrosoftAppPassword = botConfig.MicrosoftAppPassword,
                    MicrosoftAppTenantId = botConfig.MicrosoftAppTenantId
                });
            }

            return await Task.FromResult(applicationSettings);
        }
    }
}
