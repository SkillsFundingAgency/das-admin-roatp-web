namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class ConfirmAddProviderToRestrictedCourseViewModel
{
    public string LarsCode { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
    public int Ukprn { get; set; }
    public string CourseDisplayTitle { get; set; } = null!;

    public string CancelUrl { get; set; } = "#";
}

