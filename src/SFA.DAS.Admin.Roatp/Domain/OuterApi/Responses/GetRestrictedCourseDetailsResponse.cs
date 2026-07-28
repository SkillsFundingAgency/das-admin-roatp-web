using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetRestrictedCourseDetailsResponse
{
    public required string LarsCode { get; set; }
    public required string IfateReferenceNumber { get; set; }
    public required string CourseName { get; set; }
    public required string Route { get; set; }
    public LearningType LearningType { get; set; }
    public bool IsActiveAvailable { get; set; }
    public DateTime? DateLastStarts { get; set; }
    public bool IsCourseRestricted { get; set; }
    public List<ProviderCourseModel> Providers { get; set; } = [];
}
