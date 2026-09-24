namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public static class ApplicationCacheKeys
{
    public static string RestrictedApprenticeships(int ukprn) => $"RestrictedApprenticeships:{ukprn}";
}
