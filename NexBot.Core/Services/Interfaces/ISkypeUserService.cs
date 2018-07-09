namespace NexBot.Core.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using NexBot.Core.Models;

    public interface ISkypeUserService
    {
        Task<CommandResult> AddMembersToGroup(string GroupConversationId, IList<ChannelAccount> members);

        Task<CommandResult> RemoveMembersFromGroup(string GroupConversationId, IList<ChannelAccount> members);

        Task<CommandResult> DeactiveUser(string UserConversationId);

        Task<CommandResult> AddContactWithBot(Activity activity);
    }
}