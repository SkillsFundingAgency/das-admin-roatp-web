using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
public class GetRemovalReasonsResponse
{
    public List<RemovalReasonModel> ReasonsForRemoval { get; set; } = [];
}
