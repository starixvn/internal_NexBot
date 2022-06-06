namespace NexBot.Core.Services
{
    using NexBot.Core.Services.Interfaces;
    using NLog;
    using NLog.Web;

    public class LogProviderService : ILogProviderService
    {
        private readonly Logger logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public void WriteLogFileInfo(string logMessage)
        {
            logger.Error(logMessage);
        }
    }
}