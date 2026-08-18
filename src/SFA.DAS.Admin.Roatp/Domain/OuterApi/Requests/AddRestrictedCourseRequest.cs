namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;

public class AddRestrictedCourseRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string LarsCode { get; set; } = string.Empty;
}
