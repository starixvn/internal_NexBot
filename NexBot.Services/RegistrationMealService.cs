namespace NexBot.Services
{
    using System;
    using System.Threading.Tasks;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class RegistrationMealService(
        IFBMConnector fbmConnector,
        ILogProviderService logProviderService) : IRegistrationMealService
    {
        private const string successMessage = "Đặt Cơm Chay thành công ngày: ";
        private const string successMessageCancelMeal = "Hủy Đặt Cơm thành công ngày: ";
        private const string failMessage = "Đặt Cơm Chay thất bại! Vui lòng liên hệ Admin.";
        private const string failMessageCancelMeal = "Hủy Đặt Cơm thất bại! Vui lòng liên hệ Admin.";

        public async Task<string> RegistVegetarian(string conversationId, string commandRequest)
        {
            var slitCommandRequest = commandRequest.Split('_');
            var result = string.Empty;
            try
            {
                if (slitCommandRequest.Length == 5)
                {
                    _ = int.TryParse(slitCommandRequest[1], out int empId);
                    _ = int.TryParse(slitCommandRequest[2], out int companyId);
                    _ = DateTime.TryParse(slitCommandRequest[3], out DateTime onDate);

                    var registVegetarianMeal = new RegistVegetarianMealMessage
                    {
                        EmpId = empId,
                        CompanyId = companyId,
                        OnDate = onDate,
                        CallBackUrl = slitCommandRequest[4]
                    };

                    result = await RegistVegetarian(registVegetarianMeal);
                }
            }
            catch (Exception exception)
            {
                await logProviderService.SentryErrorAsync(exception.Message, exception);
                result = failMessage;
            }

            return result;
        }

        public async Task<string> RegistVegetarian(RegistVegetarianMealMessage registVegetarianMealMessage)
        {
            var result = string.Empty;
            try
            {
                var commandResult = await fbmConnector.Send(registVegetarianMealMessage);
                if (commandResult.IsSuccessful)
                {
                    result = successMessage + registVegetarianMealMessage.OnDate.ToString("dd MMM, yyyy");
                }
                else
                {
                    logProviderService.LogFileMessage(failMessage);
                    result = failMessage;
                }
            }
            catch (Exception exception)
            {
                await logProviderService.SentryErrorAsync(exception.Message, exception);
                result = failMessage;
            }

            return result;
        }

        public async Task<string> CancelMeal(CancelMealFormData cancelMealFormData)
        {
            var result = string.Empty;
            try
            {
                var commandResult = await fbmConnector.CancelMeal(cancelMealFormData);
                if (commandResult.IsSuccessful)
                {
                    result = successMessageCancelMeal + cancelMealFormData.OnDate.ToString("dd MMM, yyyy");
                }
                else
                {
                    logProviderService.LogFileMessage(commandResult.Message);
                    result = commandResult.Message;
                }
            }
            catch (Exception exception)
            {
                await logProviderService.SentryErrorAsync(exception.Message, exception);
                result = failMessageCancelMeal;
            }

            return result;
        }
    }
}