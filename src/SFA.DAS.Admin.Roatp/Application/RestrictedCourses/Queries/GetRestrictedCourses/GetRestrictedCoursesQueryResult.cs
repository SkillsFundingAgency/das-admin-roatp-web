using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Application.RestrictedCourses.Queries.GetRestrictedCourses;

public class GetRestrictedCoursesQueryResult
{
    public int TotalCount { get; set; }
    public List<RestrictedCourseModel> Courses { get; set; } = [];
}
