using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class OrganisationsService(IOuterApiClient _outerApiClient, ISessionService _sessionService) : IOrganisationsService
{
    public async Task<List<OrganisationModel>> GetOrganisations(CancellationToken cancellationToken)
    {
        var savedOrganisations = _sessionService.Get<List<OrganisationModel>>(SessionKeys.GetOrganisations);
        if (savedOrganisations != null) return savedOrganisations;

        savedOrganisations = await GetAllOrganisations(cancellationToken);
        _sessionService.Set<List<OrganisationModel>>(SessionKeys.GetOrganisations, savedOrganisations);

        return savedOrganisations;

    }

    private async Task<List<OrganisationModel>> GetAllOrganisations(CancellationToken cancellationToken)
    {
        var organisationsResponse = await _outerApiClient.GetOrganisations(cancellationToken);
        return organisationsResponse.Organisations;
    }
}
