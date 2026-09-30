using Humanizer;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class RestrictedApprenticeshipsViewModel : ICustomBackLink
{
    public const string BackLinkTextValue = "Back to organisation details";

    public string ProviderName { get; set; } = string.Empty;
    public string RestrictACourseUrl { get; set; } = "#";
    public string BackLinkUrl { get; set; } = "#";
    public string BackLinkText => BackLinkTextValue;
    public IReadOnlyList<RestrictedApprenticeshipItemViewModel> Courses { get; set; } = [];
    public bool HasActiveFilters { get; set; }
    public FiltersViewModel Filters { get; set; } = new() { Route = string.Empty };
    public PaginationViewModel Pagination { get; set; } = null!;
    public string? SuccessBannerMessage { get; set; }
    public bool HasSuccessBanner => !string.IsNullOrWhiteSpace(SuccessBannerMessage);

    public int TotalCount { get; set; }
    public bool HasCourses => TotalCount > 0;
    public bool HasNoCourses => !HasActiveFilters && !HasCourses;
    public bool HasNoFilteredResults => HasActiveFilters && !HasCourses;
    public bool ShowCourseResults => !HasNoCourses;
    public string TotalCountDescription => "course".ToQuantity(TotalCount);
}
