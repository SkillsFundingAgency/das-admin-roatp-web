using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetRestrictedCoursesResponse
{
    public List<RestrictedCourseModel> Courses { get; set; } = [];
}
