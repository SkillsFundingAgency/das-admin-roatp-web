using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetNotRestrictedApprenticeshipsResponse
{
    public List<RestrictedCourseModel> Courses { get; set; } = [];
}
