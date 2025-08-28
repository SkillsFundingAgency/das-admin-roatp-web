using Refit;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Api.Common.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.AppStart;

public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        var roatpServiceApiConfiguration = configuration.GetSection("EmployerAccountsApiConfiguration").Get<InnerApiConfiguration>()!;

        services
            .AddHttpClient()
            .AddRefitClient<IOuterApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(roatpServiceApiConfiguration.Url))
                .AddHttpMessageHandler(() => new InnerApiAuthenticationHeaderHandler(new AzureClientCredentialHelper(configuration), roatpServiceApiConfiguration.Identifier));
        return services;
    }
}
