namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using NexBot.Core;
    using NexBot.Core.Models;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class ConversationService : IConversationService
    {
        private readonly IList<BotConfigs> botConfigs;
        private readonly IBaseService baseService;

        public ConversationService(IList<BotConfigs> botConfigs, IBaseService baseService)
        {
            this.botConfigs = botConfigs;
            this.baseService = baseService;
        }

        public async Task AddConversationAsync(IActivity activity)
        {
            var firstName = string.Empty;
            if (activity.ChannelId == Constants.TelegramChannel)
            {
                var channelData = activity.GetChannelData<TelegramData>();
                firstName = channelData.message.from?.first_name;
            }

            var criteria = new InsertConversationCommand
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = activity.From.Name,
                FirstName = firstName,
                IsGroup = activity.Conversation.IsGroup,
                IsActive = true,
                IsAdmin = false,
                ConversationBotId = activity.Recipient.Id,
                ConversationBotName = activity.Recipient.Name
            };

            await baseService.ExecuteNonQueryAsync(criteria);
        }

        public async Task AddConversationAsync(InsertConversationCommand command)
        {
            await baseService.ExecuteNonQueryAsync(command);
        }

        public async Task<CommandResult> RemoveConversationAsync(IActivity activity)
        {
            var criteria = new RemoveConversationByIdCommand
            {
                ConversationId = activity.Conversation.Id
            };
            return await baseService.ExecuteNonQueryAsync(criteria);
        }

        public async Task<ConversationModel> GetConversationById(string conversationId)
        {
            var criteria = new GetConversationByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await baseService.GetDataEntityAsync<ConversationModel>(criteria);

            return conversation;
        }

        public async Task<ConversationModel> GetConversationBotById(string conversationId)
        {
            var criteria = new GetConversationBotByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await baseService.GetDataEntityAsync<ConversationModel>(criteria);

            return conversation;
        }

        public async Task<bool> CheckIsBot(string conversationId)
        {
            var conversation = await GetConversationBotById(conversationId);
            var isBot = conversation != null && !string.IsNullOrWhiteSpace(conversation.ConversationId);
            return isBot;
        }

        public async Task<IList<ConversationModel>> GetAdminConversation()
        {
            var criteria = new GetAdminConversationCriteria();

            var conversations = await baseService.GetDataAsync<ConversationModel>(criteria);

            return conversations.ToList();
        }

        public async Task<IEnumerable<ApplicationSetting>> GetApplicationSettingAsync()
        {
            var applicationSettings = botConfigs.Select(t =>
                new ApplicationSetting
                {
                    AppId = t.AppId,
                    MicrosoftAppId = t.MicrosoftAppId,
                    MicrosoftAppPassword = t.MicrosoftAppPassword,
                    MicrosoftAppTenantId = t.MicrosoftAppTenantId
                });

            return await Task.FromResult(applicationSettings);
        }

        public async Task<IEnumerable<Bot>> GetBotAsync()
        {
            var bots = new List<Bot>();
            foreach (var botConfig in botConfigs)
            {
                foreach (var channel in botConfig.Channels)
                {
                    bots.Add(new Bot
                    {
                        AppId = botConfig.AppId,
                        MicrosoftAppId = botConfig.MicrosoftAppId,
                        ChannelId = channel.ChannelId,
                        BotName = channel.BotName,
                        ConversationId = channel.BotId,
                        ChannelName = channel.ChannelName,
                        ServiceUrl = channel.ServiceUrl
                    });
                }
            }

            return await Task.FromResult(bots);
        }
    }
}