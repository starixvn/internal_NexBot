namespace NexBot
{
    using System;
    using System.Collections.Generic;
    using Fanex.Caching;
    using Fanex.Caching.Redis;
    using Fanex.Data.Repository;
    using Fanex.Data.SqlServer;
    using Fanex.Logging;
    using Fanex.Logging.Sentry;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NexBot.Bots;
    using NexBot.Bots.Interfaces;
    using NexBot.Core;
    using NexBot.Core.Repositories;
    using NexBot.Core.Services;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Models;
    using NexBot.Services;
    using NexBot.Services.Interfaces;
    using Sentry;
    using StackExchange.Redis;

    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            LogManager
               .SetDefaultLogCategory(Configuration.GetSection("Sentry:LogCategory").Value)
               .Use([
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(Configuration.GetSection("Sentry:Dsn").Value)
                    })
               ]);

            services.AddFanexSql(Configuration);

            services.AddTransient<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.AddTransient<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddTransient<ServiceClientCredentialsFactory, MultiServiceClientCredentialsFactory>();

            services.AddTransient<ITeamsBot, TeamsBot>();
            services.AddTransient<ISkypeBot, SkypeBot>();
            services.AddTransient<ITelegramBot, TelegramBot>();
            services.AddTransient<ISlackBot, SlackBot>();
            services.AddTransient<ICoreBot, CoreBot>();
            services.AddTransient<IBot, BotFactory>();

            services.AddScoped<Func<string, IChannelBot>>(serviceProvider => (channelType) =>
            {
                return channelType switch
                {
                    Constants.TeamsChannel => serviceProvider.GetService<ITeamsBot>(),
                    Constants.SkypeChannel => serviceProvider.GetService<ISkypeBot>(),
                    Constants.TelegramChannel => serviceProvider.GetService<ITelegramBot>(),
                    Constants.SlackChannel => serviceProvider.GetService<ISlackBot>(),
                    _ => serviceProvider.GetService<ICoreBot>(),
                };
            });

            RegisterCache(services, Configuration);
            RegisterService(services, Configuration);

            services.AddMvc().AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<ErrorHandlingMiddleware>();
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

        private static void RegisterCache(IServiceCollection services, IConfiguration configuration)
        {
            _ = bool.TryParse(configuration.GetSection("IsCacheDataInRedis")?.Value, out bool isCacheDataInRedis);
            services.AddTransient<ICacheService, CacheService>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            if (isCacheDataInRedis)
            {
                var multiplexer = ConnectionMultiplexer.Connect(configuration.GetSection("Fanex.Caching.Redis:ConnectionString").Value);
                services.AddDataProtection()
                .SetApplicationName(configuration.GetSection("Fanex.Caching.Redis:ApplicationName").Value)
                .PersistKeysToStackExchangeRedis(multiplexer, "Fanex.ChatBotBotService.Endpoint-Keys");
                services.AddRedisCache(configuration.GetSection("Fanex.Caching.Redis"));

                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            }
            else
            {
                services.AddMemoryCache();
            }
        }

        private static void RegisterService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();

            var botConfigsSection = configuration.GetSection("BotConfigs");
            var botConfigs = botConfigsSection.Get<IList<BotConfigs>>();
            services.AddSingleton(botConfigs);
            services.AddFanexSql(configuration);
            services.AddSingleton<IBaseRepository, BaseRepository>();
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<IBaseService, BaseService>();
            services.AddSingleton<IApplicationSettingService, ApplicationSettingService>();

            services.AddSingleton<IMessageValidationService, MessageValidationService>();
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<ILogProviderService, LogProviderService>();

            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IHRMConnector, HrmConnector>();
            services.AddScoped<IFBMConnector, FBMConnector>();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IRegistrationMealService, RegistrationMealService>();
            services.AddScoped<IRegisterMessageService, RegisterMessageService>();
            services.AddScoped<IFormAdaptiveCardService, FormAdaptiveCardService>();
            services.AddScoped<ITeamsBotHandler, TeamsBotHandler>();
            services.AddScoped<ISlackBotHandler, SlackBotHandler>();
        }
    }
}