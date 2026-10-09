using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetAllowedCoursesResponse
{
    public List<ProviderAllowedCourseModel> AllowedCourses { get; set; } = [];
}
