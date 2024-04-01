namespace NexBot.Services
{
    using System;
    using System.Threading.Tasks;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class RegistrationMealService : IRegistrationMealService
    {
        private const string successMessage = "Đặt Cơm Chay thành công ngày: ";
        private const string failMessage = "Đặt Cơm Chay thất bại! Vui lòng liên hệ Admin.";
        private readonly IFBMConnector fbmConnector;
        private readonly ILogProviderService logProviderService;

        public RegistrationMealService(
            IFBMConnector fbmConnector,
            ILogProviderService logProviderService)
        {
            this.fbmConnector = fbmConnector;
            this.logProviderService = logProviderService;
        }

        public async Task<string> RegistVegetarian(string conversationId, string commandRequest)
        {
            var slitCommandRequest = commandRequest.Split('_');
            var result = string.Empty;
            try
            {
                if (slitCommandRequest.Length == 5)
                {
                    int.TryParse(slitCommandRequest[1], out int empId);
                    int.TryParse(slitCommandRequest[2], out int companyId);
                    DateTime.TryParse(slitCommandRequest[3], out DateTime onDate);


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
    }
}