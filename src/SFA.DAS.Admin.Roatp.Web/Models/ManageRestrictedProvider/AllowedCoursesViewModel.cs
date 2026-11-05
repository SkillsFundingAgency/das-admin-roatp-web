using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public class AllowedCoursesViewModel : ICustomBackLink
{
    public const string BackLinkTextValue = "Back to organisation details";

    public RouteLearningType RouteLearningType { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string AddUrl { get; set; } = "#";
    public string BackLinkUrl { get; set; } = "#";
    public string BackLinkText => BackLinkTextValue;
    public IReadOnlyList<AllowedCourseItemViewModel> Courses { get; set; } = [];
    public bool HasActiveFilters { get; set; }
    public FiltersViewModel Filters { get; set; } = new() { Route = string.Empty };

    public int TotalCount { get; set; }
    public bool HasCourses => TotalCount > 0;
    public bool HasNoCourses => !HasActiveFilters && !HasCourses;
    public bool HasNoFilteredResults => HasActiveFilters && !HasCourses;
    public bool ShowCourseResults => !HasNoCourses;
    public string TotalCountDescription => "course".ToQuantity(TotalCount);
    public string? SuccessBannerMessage { get; set; }
    public bool HasSuccessBanner => !string.IsNullOrWhiteSpace(SuccessBannerMessage);

    public bool IsApprenticeship => RouteLearningType == RouteLearningType.Apprenticeship;
    public bool IsApprenticeshipUnit => RouteLearningType == RouteLearningType.ApprenticeshipUnit;
}
