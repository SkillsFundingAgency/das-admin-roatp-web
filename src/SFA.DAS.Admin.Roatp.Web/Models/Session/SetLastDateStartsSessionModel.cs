namespace SFA.DAS.Admin.Roatp.Web.Models.Session;

public class SetLastDateStartsSessionModel : ISessionModel
{
    public required int Ukprn { get; set; }
    public required string LarsCode { get; set; }
    public required string ProviderName { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public DateTime? CourseLastDateStarts { get; set; }
    public DateTime? ProviderLastDateStarts { get; set; }
}
