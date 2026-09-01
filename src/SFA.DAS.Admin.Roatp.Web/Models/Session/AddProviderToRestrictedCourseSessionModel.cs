namespace SFA.DAS.Admin.Roatp.Web.Models.Session;

public class AddProviderToRestrictedCourseSessionModel : ISessionModel
{
    public required string LarsCode { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public required int Ukprn { get; set; }
    public required string LegalName { get; set; }
}
