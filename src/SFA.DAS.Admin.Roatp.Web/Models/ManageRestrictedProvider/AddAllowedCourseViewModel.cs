using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public class AddAllowedCourseViewModel : AddAllowedCourseSubmitModel
{
    public int Ukprn { get; set; }
    public RouteLearningType RouteLearningType { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = "#";
    public IEnumerable<SelectListItem> Courses { get; set; } = [];

    public bool IsApprenticeship => RouteLearningType == RouteLearningType.Apprenticeship;
    public bool IsApprenticeshipUnit => RouteLearningType == RouteLearningType.ApprenticeshipUnit;
}
