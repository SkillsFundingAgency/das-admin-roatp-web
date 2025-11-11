using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IOrganisationPatchService
{
    Task<bool> OrganisationPatched(int ukprn, GetOrganisationResponse getOrganisationResponse, PatchOrganisationModel patchModel, CancellationToken cancellationToken);
}
