namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class AddProviderToRestrictedCourseSubmitModel
{
    public string? SearchTerm => string.IsNullOrWhiteSpace(Ukprn) ? string.Empty : $"{LegalName} UKPRN: {Ukprn}";
    public string? LegalName { get; set; }
    public string? Ukprn { get; set; }
}
