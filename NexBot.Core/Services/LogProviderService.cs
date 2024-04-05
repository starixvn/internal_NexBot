namespace NexBot.Core.Services
{
    using System;
    using System.Threading.Tasks;
    using NexBot.Core.Services.Interfaces;
    using NLog;
    using NLog.Web;

    public class LogProviderService(Fanex.Logging.ILogger logger) : ILogProviderService
    {
        private readonly Logger nlog = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        public void LogFileError(Exception exception)
        {
            nlog.Error(exception);
        }

        public void LogFileMessage(string message)
        {
            nlog.Error(message);
        }

        public async Task SentryErrorAsync(string errMsg, Exception exception = null)
        {
            await logger.ErrorAsync(errMsg, exception);
        }

        public async Task SentryInfoAsync(string info)
        {
            await logger.InfoAsync(info);
        }
    }
}