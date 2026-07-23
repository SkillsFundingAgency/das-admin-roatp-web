using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class RestrictedCoursesViewModel : IBackLink
{
    public int TotalCount { get; set; }
    public List<RestrictedCourseItemViewModel> Courses { get; set; } = [];

    public string TotalCountDescription => TotalCount == 1 ? "1 course" : $"{TotalCount} courses";
}
