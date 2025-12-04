using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class OrganisationTypesService(IOuterApiClient _outerApiClient, ISessionService _sessionService) : IOrganisationTypesService
{
    public async Task<List<OrganisationTypeModel>> GetOrganisationTypes(CancellationToken cancellationToken)
    {
        var savedOrganisationTypes = _sessionService.Get<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes);
        if (savedOrganisationTypes != null) return savedOrganisationTypes;

        savedOrganisationTypes = await GetAllOrganisationTypes(cancellationToken);
        _sessionService.Set<List<OrganisationTypeModel>>(SessionKeys.GetOrganisationTypes, savedOrganisationTypes);

        return savedOrganisationTypes;
    }

    private async Task<List<OrganisationTypeModel>> GetAllOrganisationTypes(CancellationToken cancellationToken)
    {
        var organisationTypesResponse = await _outerApiClient.GetOrganisationTypes(cancellationToken);
        return organisationTypesResponse.OrganisationTypes;
    }
}
