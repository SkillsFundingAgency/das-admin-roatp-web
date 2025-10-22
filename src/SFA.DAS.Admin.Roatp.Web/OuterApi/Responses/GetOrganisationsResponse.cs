using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.OuterApi.Responses;

public class GetOrganisationsResponse
{
    public List<OrganisationModel> Organisations { get; set; } = [];
}
