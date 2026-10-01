namespace SFA.DAS.Admin.Roatp.Web.Models.Session;

public class ChangeRestrictedCourseRestrictionSessionModel : ISessionModel
{
    public required int Ukprn { get; set; }
    public required string LarsCode { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public required string ProviderName { get; set; }
    public DateTime? CourseLastDateStarts { get; set; }
}
