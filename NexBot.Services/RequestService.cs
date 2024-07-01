namespace NexBot.Services
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NexBot.Core;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models.Request;
    using NexBot.Services.Interfaces;

    public class RequestService(IHRMConnector hrmConnector,
        ILogProviderService logProviderService) : IRequestService
    {
        private const string successMessage = "Request **{0}** has been **{1}** successfully!";
        private const string failMessage = "Request **{0}** has been **{1}** unsuccessfully! Please contact Admin for detail";

        public async Task<string> ProcessRequest(string commandRequest, string command)
        {
            var slitCommandRequest = commandRequest.Split('_');
            _ = long.TryParse(slitCommandRequest[1], out long requestId);

            var result = string.Empty;
            try
            {
                _ = int.TryParse(slitCommandRequest[2], out int processedBy);
                var processRequest = new ProcessRequest
                {
                    RequestId = requestId,
                    ProcessedBy = processedBy,
                    Command = command,
                    CallBackUrl = slitCommandRequest[3]
                };

                result = await ProcessRequest(processRequest);
            }
            catch (Exception exception)
            {
                await logProviderService.SentryErrorAsync(exception.Message, exception);
                result = string.Format(failMessage, requestId, GetCommandText(command));
            }

            return result;
        }

        public async Task<string> ProcessRequest(ProcessRequest processRequest)
        {
            string resultMessage;
            try
            {
                var commandResult = await hrmConnector.Send(processRequest);
                resultMessage = commandResult.IsSuccessful ? successMessage : failMessage;
            }
            catch (Exception exception)
            {
                await logProviderService.SentryErrorAsync(exception.Message, exception);
                resultMessage = failMessage;
            }

            var result = string.Format(resultMessage, "#" + processRequest.RequestId, GetCommandText(processRequest.Command));
            return result;
        }

        private static string GetCommandText(string command)
            => command.Equals(Constants.ApproveRequestCommand, StringComparison.CurrentCultureIgnoreCase) ? "approved" : "rejected";

        public async Task<string> ProcessMultipleRequests(ProcessRequest processRequest)
        {
            StringBuilder resultMessage = new();
            try
            {
                var processResults = await hrmConnector.SendMultipleRequest(processRequest);
                if (processResults.Any(i => i.IsSuccessful))
                {
                    resultMessage.Append(string.Format(successMessage,
                                        string.Join(", ", processResults.Where(r => r.IsSuccessful).Select(r => "#" + r.RequestId)),
                                        GetCommandText(processRequest.Command)));
                    if (processResults.Any(i => !i.IsSuccessful))
                    {
                        resultMessage.Append(Environment.NewLine);
                        resultMessage.Append(Environment.NewLine);
                    }
                }

                if (processResults.Any(i => !i.IsSuccessful))
                {
                    resultMessage.AppendLine(string.Format(failMessage,
                    string.Join(", ", processResults.Where(r => !r.IsSuccessful).Select(r => "#" + r.RequestId)),
                    GetCommandText(processRequest.Command)));
                }
            }
            catch (Exception exception)
            {
                await logProviderService.SentryErrorAsync(exception.Message, exception);
            }

            return resultMessage.ToString();
        }
    }
}