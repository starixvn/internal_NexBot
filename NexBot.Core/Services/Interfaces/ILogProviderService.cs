namespace NexBot.Core.Services.Interfaces
{
    using System.Threading.Tasks;
    using System;

    public interface ILogProviderService
    {
        Task SentryErrorAsync(string errMsg, Exception exception = null);

        Task SentryInfoAsync(string info);

        void LogFileMessage(string message);

        void LogFileError(Exception exception);
    }
}