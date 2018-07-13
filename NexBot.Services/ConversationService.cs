namespace NexBot.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Models;
    using NexBot.Core.Services;
    using NexBot.Criteria.Conversation;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class ConversationService : BaseService, IConversationService
    {
        public ConversationService()
        {
        }

        public async Task AddContact(Activity activity)
        {
            var criteria = new InsertConversationCriteria
            {
                ConversationId = activity.Conversation.Id,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
                UserName = activity.From.Name,
                IsGroup = activity.Conversation.IsGroup,
                IsActive = true,
                IsAdmin = false,
                ConversationBotId = activity.Recipient.Id,
                ConversationBotName = activity.Recipient.Name
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task<ConversationModel> GetContactByConversationId(string conversationId)
        {
            var criteria = new GetConversationByIdCriteria
            {
                ConversationId = conversationId
            };

            var conversation = await GetDataEntityAsync<ConversationModel>(criteria);

            return conversation;
        }

        public async Task<IList<ConversationModel>> GetAdminConversation()
        {
            var criteria = new GetAdminConversationCriteria();

            var conversations = await GetDataAsync<ConversationModel>(criteria);

            return conversations.ToList();
        }

        public async Task AddNewMemberToGroup(string groupConversationId, string userConversationId, string botConversationId)
        {
            var criteria = new InsertGroupUserConversationCriteria
            {
                GroupConversationId = groupConversationId,
                UserConversationId = userConversationId,
                ConversationBotId = botConversationId
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task RemoveMemberFromGroup(string groupConversationId, string userConversationId)
        {
            var criteria = new RemoveGroupUserConversationCriteria
            {
                GroupConversationId = groupConversationId,
                UserConversationId = userConversationId
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task<CommandResult> RemoveContact(Activity activity)
        {
            var criteria = new RemoveConversationByIdCriteria
            {
                ConversationId = activity.Conversation.Id
            };
            return await ExecuteNonQueryAsync(criteria);
        }

        public async Task<IEnumerable<ChannelAccount>> GetAllMembersInGroup(Activity activity)
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                IEnumerable<ChannelAccount> listMembers = new List<ChannelAccount>();

                var client = scope.Resolve<IConnectorClient>();
                listMembers = await client.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
                return listMembers.Where(m => !m.Id.Equals(activity.Recipient.Id));
            }
        }

        private async Task InsertMemberConversation(ChannelAccount account)
        {
            var criteria = new InsertConversationByChannelAccountCriteria
            {
                ConversationId = account.Id,
                UserName = account.Name
            };

            await ExecuteNonQueryAsync(criteria);
        }

        public async Task InsertGroupMembersInfo(IList<ChannelAccount> members)
        {
            foreach (var member in members)
            {
                await InsertMemberConversation(member);
            }
        }
    }
}