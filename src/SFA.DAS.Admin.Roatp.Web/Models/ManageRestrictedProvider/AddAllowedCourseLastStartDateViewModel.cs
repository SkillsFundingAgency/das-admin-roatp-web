using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public class AddAllowedCourseLastStartDateViewModel : SetLastDateStartsSubmitModel
{
    public int Ukprn { get; set; }
    public RouteLearningType RouteLearningType { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public required string CancelUrl { get; set; }

    public bool IsApprenticeship => RouteLearningType == RouteLearningType.Apprenticeship;
    public bool IsApprenticeshipUnit => RouteLearningType == RouteLearningType.ApprenticeshipUnit;
}
