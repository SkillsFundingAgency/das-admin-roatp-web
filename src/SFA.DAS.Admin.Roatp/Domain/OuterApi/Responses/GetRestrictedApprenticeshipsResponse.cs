using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetRestrictedApprenticeshipsResponse
{
    public List<RestrictedApprenticeshipModel> Courses { get; set; } = [];
}
