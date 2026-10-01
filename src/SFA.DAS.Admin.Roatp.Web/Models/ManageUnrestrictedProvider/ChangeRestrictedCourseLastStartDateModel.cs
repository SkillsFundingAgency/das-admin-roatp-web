namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class ChangeRestrictedCourseLastStartDateModel
{
    public int Ukprn { get; set; }
    public required string LarsCode { get; set; }
    public DateTime? LastDateStarts { get; set; }
}
