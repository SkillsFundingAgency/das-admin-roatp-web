using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IOrganisationService
{
    Task<List<OrganisationModel>> GetOrganisations(CancellationToken cancellationToken);
}
