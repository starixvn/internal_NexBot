namespace NexBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Fanex.Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NexBot.Bots;
    using NexBot.Configurations;
    using NexBot.Core;
    using NexBot.Core.Services;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Services;
    using NexBot.Services.Interfaces;
    using RestSharp;

    public class Startup
    {
        private readonly ILogger logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            this.logger = logger;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Dictionary<string, string> connections = Configuration
                .GetSection("ConnectionStrings")
                .GetChildren()
                .ToDictionary(connection => connection.Key, connection => connection.Value, StringComparer.OrdinalIgnoreCase);

            DbSettingProviderManager
               .StartNewSession()
               .UseConnectionStrings(connections)
               .UseDefaultDbSettingProvider("~/App_Data")
               .Run();
            services.Configure<AppSettings>(options => Configuration.GetSection("AppSettings").Bind(options));

            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IRestClient, RestClient>();
            services.AddScoped<IHRMConnector, HrmConnector>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IRegisterMessageService, RegisterMessageService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            BotServiceDI(services);
        }

        private void BotServiceDI(IServiceCollection services)
        {
            services.AddTransient<ICredentialProvider, CredentialProvider>();
            services.AddTransient<IBot, SkypeBotHandler>();
            var appSettingsSection = Configuration.GetSection("SkypeBotConfigurations");
            services.Configure<SkypeBotConfigurations>(appSettingsSection);
            var skypeBotConfigurations = appSettingsSection.Get<SkypeBotConfigurations>();
            services.Configure<SkypeBotConfigurations>(options => Configuration.GetSection("SkypeBotConfigurations").Bind(options));

            var botFrameworkHttpAdapter = GetBotFrameworkHttpAdapter(skypeBotConfigurations);
            services.AddSingleton(botFrameworkHttpAdapter);

            services.AddBot<SkypeBotHandler>(options =>
            {
                options.CredentialProvider = new CredentialProvider(skypeBotConfigurations);
                options.HttpClient = new HttpClient();
                options.OnTurnError = (turnContext, exception) =>
                {
                    logger.LogError($"Exception caught : {exception.Message}");
                    throw exception;
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMvc();
        }

        private static IBotFrameworkHttpAdapter GetBotFrameworkHttpAdapter(SkypeBotConfigurations botConfig)
        {
            var credentialProvider = new CredentialProvider(botConfig);
            HttpClient httpClient = new HttpClient();
            IBotFrameworkHttpAdapter botFrameworkHttpAdapter = new BotFrameworkHttpAdapter(credentialProvider, null, httpClient, null);

            return botFrameworkHttpAdapter;
        }
    }
}