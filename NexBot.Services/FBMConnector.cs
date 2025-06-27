namespace NexBot.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NexBot.Core.Models;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class FBMConnector(IHttpClientFactory httpClientFactory) : IFBMConnector
    {
        public async Task<CommandResult> Send(RegistVegetarianMealMessage registVegetarianMeal)
        {
            CommandResult commandResult;
            var baseUrl = registVegetarianMeal.CallBackUrl;
            var httpClient = httpClientFactory.CreateClient();

            var url = new Uri($"{baseUrl}/MealRegistrationVegetarian/RegistVegetarian?empId={registVegetarianMeal.EmpId}&CompanyId={registVegetarianMeal.CompanyId}&OnDate={registVegetarianMeal.OnDate}");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        commandResult = JsonConvert.DeserializeObject<CommandResult>(result);
                    }
                    else
                    {
                        commandResult = new CommandResult
                        {
                            Code = 1,
                            Message = response.ReasonPhrase
                        };
                    }
                }
            }

            return commandResult;
        }

        public async Task<CommandResult> CancelMeal(CancelMealFormData cancelMealFormData)
        {
            CommandResult commandResult;
            var baseUrl = cancelMealFormData.CallBackUrl;
            var httpClient = httpClientFactory.CreateClient();

            var url = new Uri($"{baseUrl}/MealCancelRegister/CancelRegisterMealByDate?SysId={cancelMealFormData.SysId}&OnDate={cancelMealFormData.OnDate}&MealId={cancelMealFormData.MealId}");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    commandResult = JsonConvert.DeserializeObject<CommandResult>(result);
                }
                else
                {
                    commandResult = new CommandResult
                    {
                        Code = 1,
                        Message = response.ReasonPhrase
                    };
                }
            }

            return commandResult;
        }

    }
}