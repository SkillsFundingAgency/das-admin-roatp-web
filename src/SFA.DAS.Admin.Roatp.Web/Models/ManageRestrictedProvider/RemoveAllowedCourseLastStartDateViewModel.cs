using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public class RemoveAllowedCourseLastStartDateViewModel
{
    public required int Ukprn { get; set; }
    public required RouteLearningType RouteLearningType { get; set; }
    public required string LarsCode { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public required string CancelUrl { get; set; }
}
