namespace NexBot.Core.Repositories
{
    using System.Threading.Tasks;
    using Fanex.Data.Repository;
    using NexBot.Core.Models;

    public interface IBaseRepository
    {
        Task<CommandResult> ExecuteNonQueryAsync(INonQueryCommand criteria);
    }
}