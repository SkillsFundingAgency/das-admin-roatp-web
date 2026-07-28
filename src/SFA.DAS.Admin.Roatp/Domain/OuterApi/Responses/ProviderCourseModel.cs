namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class ProviderCourseModel
{
    public int Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public DateTime? DateLastStarts { get; set; }
}
