using System.Security.Claims;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class UserExtensions
{
    private const string UnknownUserName = "Unknown";

    public static string UserDisplayName(this ClaimsPrincipal user)
    {
        var givenNameClaim = user.GivenName();
        var surnameClaim = user.Surname();

        return $"{givenNameClaim} {surnameClaim}";
    }

    public static string GivenName(this ClaimsPrincipal user)
    {
        var identity = user.Identities.FirstOrDefault();
        var givenNameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        Claim? givenName = identity?.Claims.FirstOrDefault(x => x.Type == givenNameClaim || x.Type == "given_name");

        return givenName?.Value ?? UnknownUserName;
    }

    public static string Surname(this ClaimsPrincipal user)
    {
        var identity = user.Identities.FirstOrDefault();
        var surnameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        Claim? surname = identity?.Claims.FirstOrDefault(x => x.Type == surnameClaim || x.Type == "family_name");

        return surname?.Value ?? UnknownUserName;
    }

    public static string UserId(this ClaimsPrincipal user)
    {
        var upnClaimName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
        Claim? upn = user.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == upnClaimName || x.Type == "email");
        return upn?.Value ?? $"{UnknownUserName}-{UnknownUserName}";
    }

    public static bool HasValidRole(this ClaimsPrincipal user)
    {
        return
            user.IsInRole(Roles.RoatpAdminTeam)
            || user.IsInRole(Roles.RoatpGatewayAssessorTeam)
            || user.IsInRole(Roles.RoatpFinancialAssessorTeam)
            || user.IsInRole(Roles.RoatpAssessorTeam)
            || user.IsInRole(Roles.RoatpApplicationOversightTeam);
    }
}
