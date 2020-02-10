using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SoCreate.Extensions.Logging.ActivityLogger.LoggingProvider;

namespace SoCreate.Extensions.Logging
{
    public class ServiceLoggingConfiguration
    {
        public bool LogToApplicationInsights { get; private set; } = true;

        public bool LogToActivityLogger { get; private set; } = false;
        
        public bool RegisterActivityLogger { get; private set; } = false;

        public bool ApplicationInsightsTelemetry { get; private set; }

        public IConfiguration? Configuration { get; }

        public Type? UserProvider { get; private set; }

        public Type? TenantProvider { get; private set; }

        public Type? AccountProvider { get; private set; }
        
        public Type? AccountProviderType { get; private set; }

        public ServiceLoggingConfiguration(HostBuilderContext hostBuilderContext)
        {
            var isWebApp = hostBuilderContext.Properties.ContainsKey("UseStartup.StartupType");
            if (isWebApp)
            {
                ApplicationInsightsTelemetry = true;
            }

            Configuration = hostBuilderContext.Configuration;
        }

        public ServiceLoggingConfiguration(WebHostBuilderContext webHostBuilderContext)
        {
            ApplicationInsightsTelemetry = true;
            Configuration = webHostBuilderContext.Configuration;
        }

        public ServiceLoggingConfiguration AddApplicationInsights()
        {
            LogToApplicationInsights = true;
            return this;
        }

        public ServiceLoggingConfiguration AddApplicationInsights(Action<AppInsightsConfiguration> action)
        {
            LogToApplicationInsights = true;
            var appInsightsConfiguration = new AppInsightsConfiguration();
            action.Invoke(appInsightsConfiguration);
            
            UserProvider = appInsightsConfiguration.UserProvider ?? throw new Exception("UserProvider must be configured");

            return this;
        }

        public ServiceLoggingConfiguration AddActivityLogging<TKeyType>(
            Action<ActivityLoggerConfiguration<TKeyType>> action,
            bool loggingOn = true)
        {
            LogToActivityLogger = loggingOn;
            RegisterActivityLogger = true;
            var activityLoggerConfiguration = new ActivityLoggerConfiguration<TKeyType>();
            action.Invoke(activityLoggerConfiguration);
            
            AccountProviderType = typeof(IAccountProvider<TKeyType>);
            UserProvider = activityLoggerConfiguration.UserProvider ?? throw new Exception("UserProvider must be configured");
            AccountProvider = activityLoggerConfiguration.AccountProvider ?? throw new Exception("AccountProvider must be configured");
            TenantProvider = activityLoggerConfiguration.TenantProvider ?? throw new Exception("TenantProvider must be configured");

            return this;
        }
    }

    public class AppInsightsConfiguration
    {
        public AppInsightsConfiguration WithUserProvider<TUserProvider>() where TUserProvider : IUserProvider
        {
            UserProvider = typeof(TUserProvider);
            return this;
        }

        public Type UserProvider { get; set; } = null!;
    }

    public class ActivityLoggerConfiguration<TKeyType>
    {
        public ActivityLoggerConfiguration<TKeyType> WithUserProvider<TUserProvider>() where TUserProvider : IUserProvider
        {
            UserProvider = typeof(TUserProvider);
            return this;
        }

        public ActivityLoggerConfiguration<TKeyType> WithTenantProvider<TTenantProvider>() where TTenantProvider : ITenantProvider
        {
            TenantProvider = typeof(TTenantProvider);
            return this;
        }

        public ActivityLoggerConfiguration<TKeyType> WithAccountProvider<TAccountProvider>() where TAccountProvider : IAccountProvider<TKeyType>
        {
            AccountProvider = typeof(TAccountProvider);
            return this;
        }

        public Type UserProvider { get; private set; } = null!;

        public Type TenantProvider { get; private set; } = null!;

        public Type AccountProvider { get; private set; } = null!;
    }
}