using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class RestrictedCoursesViewModel : IBackLink
{
    public int TotalCount { get; set; }
    public IEnumerable<RestrictedCourseItemViewModel> Courses { get; set; } = [];
    public bool HasActiveFilters { get; set; }
    public FiltersViewModel Filters { get; set; } = new() { Route = string.Empty };
    public PaginationViewModel Pagination { get; set; } = null!;

    public bool HasCourses => TotalCount > 0;
    public bool HasNoCourses => !HasActiveFilters && !HasCourses;
    public bool HasNoFilterResults => HasActiveFilters && !HasCourses;
    public bool ShowCourseResults => !HasNoCourses;

    public string TotalCountDescription => "course".ToQuantity(TotalCount);

    public static implicit operator RestrictedCoursesViewModel(GetRestrictedCoursesResponse response) => new()
    {
        TotalCount = response?.Courses.Count ?? 0,
        Courses = response?.Courses.Select(course => (RestrictedCourseItemViewModel)course) ?? []
    };
}
