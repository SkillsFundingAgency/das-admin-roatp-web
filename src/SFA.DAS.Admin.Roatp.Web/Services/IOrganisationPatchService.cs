using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IOrganisationPatchService
{
    Task<bool> OrganisationPatched(int ukprn, PatchOrganisationModel patchModel, CancellationToken cancellationToken);
}
