using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexBot.Core;
using NexBot.Core.Models;
using NexBot.Core.Services;
using NexBot.Criteria.RegisterMessage;
using NexBot.Models.CategoryMessage;
using NexBot.Services.Interfaces;

namespace NexBot.Services
{
    public class RegisterMessageService : BaseService, IRegisterMessageService
    {
        public async Task<string> GetCategoryMessages(string conversationId)
        {
            var listEvent = await GetAllCategoryMessage(conversationId);
            var message = string.Empty;
            if (listEvent.Count() > 0)
            {
                var count = 1;
                message += $"List category messages: {Constants.NewLine}";
                foreach (var item in listEvent)
                {
                    message += $"{count}. **Name: ** {item.Name}; **ID: ** {item.CategoryId}; **Registered: ** {item.IsRegistered.ToString()} {Constants.NewLine}";
                    count++;
                }
                message += $"To register an event to receive message, please tag me with command `register_message [EventId]` {Constants.NewLine}";
                message += $"To remove a registered event to receive message, please tag me with command `remove_registed_message [EventId]` {Constants.NewLine}";
            }
            else
            {
                message += "No event to display";
            }

            return message;
        }

        private async Task<IEnumerable<CategoryMessage>> GetAllCategoryMessage(string conversationId)
        {
            var criteria = new GetCategoryMessageCriteria
            {
                ConversationId = conversationId
            };
            return await GetDataAsync<CategoryMessage>(criteria);
        }

        public async Task<CommandResult> RegisterEventMessage(string conversationId, int categoryId)
        {
            var criteria = new InsertConversationRegisterCriteria
            {
                ConversationId = conversationId,
                CategoryId = categoryId
            };

            var result = await ExecuteNonQueryAsync(criteria);

            return result;
        }

        public async Task<CommandResult> RemoveRegisteredMessage(string conversationId, int categoryId)
        {
            var criteria = new RemoveConversationRegisterCriteria
            {
                ConversationId = conversationId,
                CategoryId = categoryId
            };

            var result = await ExecuteNonQueryAsync(criteria);

            return result;
        }
    }
}