namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;

public class UpsertProviderAllowedCourseRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public DateTime? LastDateStarts { get; set; }
}
