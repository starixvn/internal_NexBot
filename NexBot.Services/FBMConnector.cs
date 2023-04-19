namespace NexBot.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NexBot.Models;
    using NexBot.Services.Interfaces;

    public class FBMConnector : IFBMConnector
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public FBMConnector(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> Send(RegistVegetarianMealMessage registVegetarianMeal)
        {
            string result = string.Empty;
            var baseUrl = registVegetarianMeal.CallBackUrl;
            var httpClient = httpClientFactory.CreateClient();

            var url = new Uri($"{baseUrl}/MealRegistrationVegetarian/RegistVegetarian?empId={registVegetarianMeal.EmpId}&CompanyId={registVegetarianMeal.CompanyId}&OnDate={registVegetarianMeal.OnDate}");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                using (var response = await httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return result;
        }
    }
}