namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using NexBot.Core;
    using NexBot.Core.Models;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Models.Request;
    using NexBot.Models.Slack;
    using NexBot.Services.Interfaces;
    using static System.Text.RegularExpressions.Regex;

    public class ConversationService(IList<BotConfigs> botConfigs, IBaseService baseService, IHRMConnector hrmConnector) : IConversationService
    {
        public async Task AddConversationAsync(IActivity activity)
        {
            var firstName = string.Empty;
            var userName = activity.From.Name;

            if (activity.ChannelId == Constants.TelegramChannel)
            {
                var channelData = activity.GetChannelData<TelegramData>();
                firstName = channelData.message.from?.first_name;
            }
            if (activity.ChannelId == Constants.SlackChannel)
            {
                var channelData = activity.ChannelData?.ToString();
                if (!string.IsNullOrEmpty(channelData))
                {
                    var slackData = JsonConvert.DeserializeObject<SlackEventPayload>(channelData);
                    firstName = slackData.SlackMessage.Event.User;
                }
                userName = Replace(userName, @"\d+", "");
            }

            var criteria = new InsertConversationCommand
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = userName,
                FirstName = firstName,
                IsGroup = activity.Conversation.IsGroup,
                IsActive = true,
                IsAdmin = false,
                ConversationBotId = activity.Recipient.Id,
                ConversationBotName = activity.Recipient.Name
            };

            var res = await baseService.ExecuteNonQueryAsync(criteria);
            if (res.IsSuccessful)
            {
                await hrmConnector.MappingConversation(new MappingConversationRequest
                {
                    BotId = activity.Recipient.Id,
                    BotName = activity.Recipient.Name,
                    ConversationId = activity.Conversation.Id,
                    Username = userName
                });
            }
        }

        public async Task AddConversationAsync(InsertConversationCommand command)
        {
            var res = await baseService.ExecuteNonQueryAsync(command);
            if (res.IsSuccessful)
            {
                await hrmConnector.MappingConversation(new MappingConversationRequest
                {
                    BotId = command.ConversationBotId,
                    BotName = command.ConversationBotName,
                    ConversationId = command.ConversationId,
                    Username = command.UserName
                });
            }
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

        public async Task<IList<ConversationModel>> GetAdminConversation()
        {
            var criteria = new GetAdminConversationCriteria();

            var conversations = await baseService.GetDataAsync<ConversationModel>(criteria);

            return conversations.ToList();
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