namespace NexBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NexBot.Bots;
    using NexBot.Core;
    using NexBot.Core.Services;
    using NexBot.Core.Services.Interfaces;
    using NexBot.Services;
    using NexBot.Services.Interfaces;
    using RestSharp;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddTransient<IBot, SkypeBotHandler>();
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
    }
}