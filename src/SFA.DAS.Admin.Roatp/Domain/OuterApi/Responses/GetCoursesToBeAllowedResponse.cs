using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetCoursesToBeAllowedResponse
{
    public List<CourseToBeAllowedModel> Courses { get; set; } = [];
}
