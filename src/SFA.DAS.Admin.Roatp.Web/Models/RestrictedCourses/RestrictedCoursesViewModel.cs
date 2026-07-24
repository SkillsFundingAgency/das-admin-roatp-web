using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class RestrictedCoursesViewModel : IBackLink
{
    public int TotalCount { get; set; }
    public List<RestrictedCourseItemViewModel> Courses { get; set; } = [];

    public string TotalCountDescription => TotalCount == 1 ? "1 course" : $"{TotalCount} courses";

    public static implicit operator RestrictedCoursesViewModel(GetRestrictedCoursesResponse response) => new()
    {
        TotalCount = response?.Courses.Count ?? 0,
        Courses = response?.Courses.Select(course => (RestrictedCourseItemViewModel)course).ToList() ?? []
    };
}
