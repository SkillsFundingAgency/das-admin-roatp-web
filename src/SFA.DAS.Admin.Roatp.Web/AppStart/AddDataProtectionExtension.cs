using Microsoft.AspNetCore.DataProtection;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using StackExchange.Redis;

namespace SFA.DAS.Admin.Roatp.Web.AppStart;

public static class AddDataProtectionExtension
{
    public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(ApplicationConfiguration))
            .Get<ApplicationConfiguration>();

        if (config != null
            && !string.IsNullOrEmpty(config.DataProtectionKeysDatabase)
            && !string.IsNullOrEmpty(config.RedisConnectionString))
        {
            var redisConnectionString = config.RedisConnectionString;
            var dataProtectionKeysDatabase = config.DataProtectionKeysDatabase;

            var redis = ConnectionMultiplexer.Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

            services.AddDataProtection()
                .SetApplicationName("das-admin-service-web")
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
        }

        return services;
    }
}
