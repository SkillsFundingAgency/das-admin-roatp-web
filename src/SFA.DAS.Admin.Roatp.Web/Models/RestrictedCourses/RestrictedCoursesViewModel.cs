using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class RestrictedCoursesViewModel : IBackLink
{
    public int TotalCount { get; set; }
    public IEnumerable<RestrictedCourseItemViewModel> Courses { get; set; } = [];

    public bool HasCourses => Courses.Any();
    public bool HasNoCourses => !HasCourses;

    public string TotalCountDescription => "course".ToQuantity(TotalCount);

    public static implicit operator RestrictedCoursesViewModel(GetRestrictedCoursesResponse response) => new()
    {
        TotalCount = response?.Courses.Count ?? 0,
        Courses = response?.Courses.Select(course => (RestrictedCourseItemViewModel)course) ?? []
    };
}
