namespace NexBot
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Rest;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using System.Linq;
    using NexBot.Services.Interfaces;
    using Microsoft.Extensions.Logging;
    using NexBot.Models;

    internal enum MicrosoftAppType
    {
        MultiTenant,

        SingleTenant,

        UserAssignedMsi
    }

    public class MultiServiceClientCredentialsFactory : ServiceClientCredentialsFactory
    {
        private readonly List<ServiceClientCredentialsFactory> _inners = new List<ServiceClientCredentialsFactory>();

        public MultiServiceClientCredentialsFactory(IApplicationSettingService applicationSettingService, HttpClient httpClient = null, ILogger logger = null)
        {
            var applicationSettings = applicationSettingService.GetApplicationSettingAsync().Result;

            foreach (var applicationSetting in applicationSettings)
            {
                _inners.Add(GetServiceClientCredentialsFactory(applicationSetting, httpClient, logger));
            }
        }

        private ServiceClientCredentialsFactory GetServiceClientCredentialsFactory(ApplicationSetting applicationSetting, HttpClient httpClient = null, ILogger logger = null)
        {
            var appType = applicationSetting.MicrosoftAppType;
            var appId = applicationSetting.MicrosoftAppId;
            var password = applicationSetting.MicrosoftAppPassword;
            var tenantId = applicationSetting.MicrosoftAppTenantId;

            var parsedAppType = Enum.TryParse(appType, ignoreCase: true, out MicrosoftAppType parsed)
                ? parsed
                : MicrosoftAppType.MultiTenant; // default

            switch (parsedAppType)
            {
                case MicrosoftAppType.UserAssignedMsi:
                    if (string.IsNullOrWhiteSpace(appId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is required for MSI in configuration.");
                    }

                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppTenantIdKey} is required for MSI in configuration.");
                    }

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppPasswordKey} must not be set for MSI in configuration.");
                    }

                    return new ManagedIdentityServiceClientCredentialsFactory(appId, httpClient, logger);

                case MicrosoftAppType.SingleTenant:
                    if (string.IsNullOrWhiteSpace(appId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is required for SingleTenant in configuration.");
                    }

                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppTenantIdKey} is required for SingleTenant in configuration.");
                    }

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppPasswordKey} is required for SingleTenant in configuration.");
                    }

                    return new PasswordServiceClientCredentialFactory(appId, password, tenantId, httpClient, logger);

                default: // MultiTenant
                    return new PasswordServiceClientCredentialFactory(appId, password, tenantId: string.Empty, httpClient, logger);
            }
        }

        public override Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_inners.Exists(t => t.IsValidAppIdAsync(appId, cancellationToken).Result));
        }

        public override Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inners.Any());
        }

        public override Task<ServiceClientCredentials> CreateCredentialsAsync(
            string appId, string audience, string loginEndpoint, bool validateAuthority, CancellationToken cancellationToken)
        {
            var _inner = _inners.SingleOrDefault(t => t.IsValidAppIdAsync(appId, cancellationToken).Result);
            if (_inner == null)
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is invalid in configuration.");
            }

            return _inner.CreateCredentialsAsync(
                appId, audience, loginEndpoint, validateAuthority, cancellationToken);
        }
    }
}
