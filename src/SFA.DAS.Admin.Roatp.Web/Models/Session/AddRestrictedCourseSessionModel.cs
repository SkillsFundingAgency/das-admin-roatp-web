namespace SFA.DAS.Admin.Roatp.Web.Models.Session;

public class AddRestrictedCourseSessionModel : ISessionModel
{
    public required string LarsCode { get; set; }
    public required string DisplayName { get; set; }
}
