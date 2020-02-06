using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;
using SoCreate.Extensions.Logging.Options;

namespace SoCreate.Extensions.Logging
{
    public class ServiceLoggingConfiguration
    {
        public LoggerOptions LoggerOptions { get; set; }

        public bool AddApplicationInsightsTelemetry { get; set; }

        public IConfiguration? Configuration { get; set; }

        public Type? UserProvider { get; set; }

        public Type? TenantProvider { get; set; }

        public Type? AccountProvider { get; set; }

        public ServiceLoggingConfiguration(HostBuilderContext hostBuilderContext, LoggerOptions loggerOptions)
        {
            var isWebApp = hostBuilderContext.Properties.ContainsKey("UseStartup.StartupType");
            if (isWebApp)
            {
                AddApplicationInsightsTelemetry = true;
            }

            Configuration = hostBuilderContext.Configuration;
            LoggerOptions = loggerOptions;
        }

        public ServiceLoggingConfiguration(WebHostBuilderContext webHostBuilderContext, LoggerOptions loggerOptions)
        {
            AddApplicationInsightsTelemetry = true;
            Configuration = webHostBuilderContext.Configuration;
            LoggerOptions = loggerOptions;
        }

        public ServiceLoggingConfiguration WithUserProvider(Type userProvider)
        {
            if (typeof(IUserProvider).IsAssignableFrom(userProvider))
            {
                UserProvider = userProvider;
            }
            else
            {
                throw new Exception("Type must be a subclass of IUserProvider");
            }

            return this;
        }

        public ServiceLoggingConfiguration WithAccountProvider<TKeyType>(Type accountProvider)
        {
            if (typeof(IAccountProvider<TKeyType>).IsAssignableFrom(accountProvider))
            {
                AccountProvider = accountProvider;
            }
            else
            {
                throw new Exception("Type must be a subclass of IAccountProvider");
            }

            return this;
        }

        public ServiceLoggingConfiguration WithTenantProvider(Type tenantProvider)
        {
            if (tenantProvider.GetInterface(nameof(ITenantProvider)) != null)
            {
                TenantProvider = tenantProvider;
            }
            else
            {
                throw new Exception("Type must be a subclass of ITenantProvider");
            }

            return this;
        }
    }
}