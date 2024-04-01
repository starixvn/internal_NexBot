namespace NexBot.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NexBot.Models;

    public interface IApplicationSettingService
    {
        Task<IEnumerable<ApplicationSetting>> GetApplicationSettingAsync();
    }
}
