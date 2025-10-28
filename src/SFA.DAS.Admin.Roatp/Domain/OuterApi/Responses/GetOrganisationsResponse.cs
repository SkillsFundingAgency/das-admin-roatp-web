using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
public class GetOrganisationsResponse
{
    public List<OrganisationModel> Organisations { get; set; } = [];
}
