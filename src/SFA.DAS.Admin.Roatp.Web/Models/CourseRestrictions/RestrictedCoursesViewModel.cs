using Humanizer;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;

namespace SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

public class RestrictedCoursesViewModel : IBackLink
{
    public int TotalCount { get; set; }
    public IEnumerable<RestrictedCourseItemViewModel> Courses { get; set; } = [];
    public bool HasActiveFilters { get; set; }
    public FiltersViewModel Filters { get; set; } = new() { Route = string.Empty };
    public PaginationViewModel Pagination { get; set; } = null!;

    public bool HasCourses => TotalCount > 0;
    public bool HasNoCourses => !HasActiveFilters && !HasCourses;
    public bool HasNoFilteredResults => HasActiveFilters && !HasCourses;
    public bool ShowCourseResults => !HasNoCourses;

    public string TotalCountDescription => "course".ToQuantity(TotalCount);
}
