namespace NexBot.Services
{
    using System;
    using System.Threading.Tasks;
    using NexBot.Core.Services;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class RegistrationMealService : BaseService, IRegistrationMealService
    {
        private readonly IConversationService conversationService;
        private readonly IFBMConnector fbmConnector;

        public RegistrationMealService(IConversationService conversationService,
            IFBMConnector fbmConnector)
        {
            this.conversationService = conversationService;
            this.fbmConnector = fbmConnector;
        }

        public async Task RegistVegetarian(string conversationId, string commandRequest)
        {
            var slitCommandRequest = commandRequest.Split('_');
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

                await fbmConnector.Send(registVegetarianMeal);
            }
        }
    }
}