namespace NexBot.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Models;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Criteria.SkypeUser;

    public class SkypeUserService : ISkypeUserService
    {
        private IBaseService baseService;

        public SkypeUserService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<CommandResult> AddContactWithBot(Activity activity)
        {
            var criteria = new AddUserContactCriteriaCriteria
            {
                SkypeDisplayName = activity.From.Name,
                ConversationId = activity.Conversation.Id
            };

            return await baseService.ExecuteNonQueryAsync(criteria).ConfigureAwait(false);
        }

        public async Task<CommandResult> AddMembersToGroup(string GroupConversationId, IList<ChannelAccount> members)
        {
            var commandResult = new CommandResult
            {
                Code = 0
            };

            try
            {
                foreach (var member in members)
                {
                    var criteria = new AddNewUserToGroupCriteria
                    {
                        GroupConversationId = GroupConversationId,
                        UserConversationId = member.Id
                    };

                    await baseService.ExecuteNonQueryAsync(criteria);
                }
            }
            catch (Exception ex)
            {
                commandResult.Code = -1;
                commandResult.Message = ex.Message;
            }

            return commandResult;
        }

        public async Task<CommandResult> DeactiveUser(string UserConversationId)
        {
            var criteria = new DeactiveUserCriteria
            {
                UserConversationId = UserConversationId
            };

            return await baseService.ExecuteNonQueryAsync(criteria);
        }

        public async Task<CommandResult> RemoveMembersFromGroup(string GroupConversationId, IList<ChannelAccount> members)
        {
            var commandResult = new CommandResult
            {
                Code = 0
            };
            try
            {
                foreach (var member in members)
                {
                    var criteria = new RemoveUserFromGroupCriteria
                    {
                        GroupConversationId = GroupConversationId,
                        UserConversationId = member.Id
                    };

                    await baseService.ExecuteNonQueryAsync(criteria);
                }
            }
            catch (Exception ex)
            {
                commandResult.Code = -1;
                commandResult.Message = ex.Message;
            }

            return commandResult;
        }
    }
}