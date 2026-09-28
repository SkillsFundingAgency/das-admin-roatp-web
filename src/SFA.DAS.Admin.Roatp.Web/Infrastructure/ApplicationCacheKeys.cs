namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public static class ApplicationCacheKeys
{
    public const string CoursesCacheKey = "Courses";
    public static string RestrictedApprenticeships(int ukprn) => $"RestrictedApprenticeships:{ukprn}";
}
