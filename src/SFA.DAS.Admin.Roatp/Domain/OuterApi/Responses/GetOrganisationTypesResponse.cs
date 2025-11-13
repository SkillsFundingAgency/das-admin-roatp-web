using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

public class GetOrganisationTypesResponse
{
    public List<OrganisationTypeModel> OrganisationTypes { get; set; } = [];
}