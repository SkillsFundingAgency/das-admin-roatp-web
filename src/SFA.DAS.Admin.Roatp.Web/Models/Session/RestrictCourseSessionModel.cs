namespace SFA.DAS.Admin.Roatp.Web.Models.Session;

public class RestrictCourseSessionModel : ISessionModel
{
    public required int Ukprn { get; set; }
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public required string DisplayTitle { get; set; }
}
