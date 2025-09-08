using SFA.DAS.Admin.Roatp.Web.Helpers;
using SFA.DAS.DfESignIn.Auth.AppStart;
using SFA.DAS.DfESignIn.Auth.Enums;

namespace SFA.DAS.Admin.Roatp.Web.AppStart;

public static class AddAuthenticationExtension
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAndConfigureDfESignInAuthentication(
            configuration,
            "SFA.DAS.AdminService.Web.Auth",
            typeof(CustomServiceRole),
            ClientName.RoatpServiceAdmin,
            "/SignOut",
            "/SignedOut");
        return services;
    }
}
