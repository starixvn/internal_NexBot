namespace NexBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Fanex.Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NexBot.Bots;
    using NexBot.Configurations;
    using NexBot.Core.Services;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Services;
    using NexBot.Services.Interfaces;

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

            services.AddHttpClient();
            services.AddControllers();
            services.AddMvc().AddNewtonsoftJson();
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IHRMConnector, HrmConnector>();
            services.AddScoped<IFBMConnector, FBMConnector>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IRegistrationMealService, RegistrationMealService>();
            services.AddScoped<IRegisterMessageService, RegisterMessageService>();
            services.AddSingleton<ILogProviderService, LogProviderService>();
            services.AddScoped<ISkypeBot, SkypeBot>();
            services.AddScoped<ITelegramBot, TelegramBot>();

            BotServiceDI(services);
        }

        private void BotServiceDI(IServiceCollection services)
        {
            services.AddTransient<ICredentialProvider, CredentialProvider>();
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();
            services.AddSingleton<IStorage, MemoryStorage>();

            services.AddTransient<IBot, BotHandler>();
            var appSettingsSection = Configuration.GetSection("SkypeBotConfigurations");
            services.Configure<BotConfigurations>(appSettingsSection);
            var skypeBotConfigurations = appSettingsSection.Get<BotConfigurations>();
            services.Configure<BotConfigurations>(options => Configuration.GetSection("SkypeBotConfigurations").Bind(options));

            var botFrameworkHttpAdapter = GetBotFrameworkHttpAdapter(skypeBotConfigurations);
            services.AddSingleton(botFrameworkHttpAdapter);

            services.AddBot<BotHandler>(options =>
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static IBotFrameworkHttpAdapter GetBotFrameworkHttpAdapter(BotConfigurations botConfig)
        {
            var credentialProvider = new CredentialProvider(botConfig);
            HttpClient httpClient = new();
            IBotFrameworkHttpAdapter botFrameworkHttpAdapter = new BotFrameworkHttpAdapter(credentialProvider, null, httpClient, null);

            return botFrameworkHttpAdapter;
        }
    }
}