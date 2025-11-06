using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Admin.Roatp.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        var outerApiConfig = configuration
            .GetSection(nameof(AdminRoatpOuterApiConfiguration))
            .Get<AdminRoatpOuterApiConfiguration>();

        services.AddTransient<ISessionService, SessionService>();
        services.AddTransient<IOrganisationsService, OrganisationsService>();

        services.AddOuterApi(outerApiConfig!);

        return services;
    }

    private static void AddOuterApi(this IServiceCollection services, AdminRoatpOuterApiConfiguration configuration)
    {
        services.AddTransient<IApimClientConfiguration>((_) => configuration);

        services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
        services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
        services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

        var defaultSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() }
        };
        var settings = new RefitSettings(new NewtonsoftJsonContentSerializer(defaultSettings));

        services
            .AddRefitClient<IOuterApiClient>(settings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration.ApiBaseUrl))
            .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
    }
}
