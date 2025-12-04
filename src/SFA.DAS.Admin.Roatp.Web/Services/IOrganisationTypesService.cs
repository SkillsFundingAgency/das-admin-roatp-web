using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IOrganisationTypesService
{
    Task<List<OrganisationTypeModel>> GetOrganisationTypes(CancellationToken cancellationToken);
}
