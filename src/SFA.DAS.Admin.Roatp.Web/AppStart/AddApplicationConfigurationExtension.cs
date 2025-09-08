using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Admin.Roatp.Web.AppStart;

public static class AddApplicationConfigurationExtension
{
    public static IConfigurationRoot AddApplicationConfiguration(this IConfiguration config, IServiceCollection services)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(config)
            .AddEnvironmentVariables();

        configBuilder.AddAzureTableStorage(options =>
        {
            options.ConfigurationKeys = config["ConfigNames"]!.Split(",");
            options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
        });

        var configuration = configBuilder.Build();

        services.Configure<ApplicationConfiguration>(configuration.GetSection(nameof(ApplicationConfiguration)));

        return configuration;
    }
}
