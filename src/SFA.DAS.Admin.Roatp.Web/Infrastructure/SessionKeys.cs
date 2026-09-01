namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public static class SessionKeys
{
    public const string GetOrganisations = "GetOrganisations";
    public const string UpdateSupportingProviderCourseTypes = "UpdateSupportingProviderCourseTypes";
    public const string AddProvider = "AddProvider";
    public const string GetOrganisationTypes = "GetOrganisationTypes";
   
    public const string AddRestrictedCourse = "AddRestrictedCourse";

    public static string NotAllowedProvidersForRestrictedCourse(string larsCode)
        => $"NotAllowedProvidersForRestrictedCourse:{larsCode}";
}
