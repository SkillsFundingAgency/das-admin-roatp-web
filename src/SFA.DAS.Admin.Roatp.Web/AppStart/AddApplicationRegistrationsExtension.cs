using Refit;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Admin.Roatp.Web.AppStart;

public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        var outerApiConfig = configuration
            .GetSection(nameof(AdminRoatpOuterApiConfiguration))
            .Get<AdminRoatpOuterApiConfiguration>();

        services.AddTransient<ISessionService, SessionService>();

        services.AddOuterApi(outerApiConfig!);

        return services;
    }

    private static void AddOuterApi(this IServiceCollection services, AdminRoatpOuterApiConfiguration configuration)
    {
        services.AddTransient<IApimClientConfiguration>((_) => configuration);

        services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
        services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
        services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

        services
            .AddRefitClient<IOuterApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration.ApiBaseUrl))
            .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
    }
}
