using Microsoft.AspNetCore.JsonPatch;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class OrganisationPatchService(IOuterApiClient _outerApiClient, IHttpContextAccessor _contextAccessor) : IOrganisationPatchService
{
    public async Task<bool> OrganisationPatched(int ukprn, PatchOrganisationModel patchModel, CancellationToken cancellationToken)
    {
        var organisationResponse = await _outerApiClient.GetOrganisation(ukprn.ToString(), cancellationToken);

        var patchDoc = new JsonPatchDocument<PatchOrganisationModel>();

        if (organisationResponse.Status != patchModel.Status)
        {
            patchDoc.Replace(o => o.Status, patchModel.Status);
        }

        if (organisationResponse.OrganisationTypeId != patchModel.OrganisationTypeId)
        {
            patchDoc.Replace(o => o.OrganisationTypeId, patchModel.OrganisationTypeId);
        }

        if (organisationResponse.ProviderType != patchModel.ProviderType)
        {
            patchDoc.Replace(o => o.ProviderType, patchModel.ProviderType);
        }

        if (organisationResponse.RemovedReasonId != patchModel.RemovedReasonId)
        {
            patchDoc.Replace(o => o.RemovedReasonId, patchModel.RemovedReasonId);
        }

        if (!ChangeMade(patchModel, organisationResponse)) return false;

        string userDisplayName = _contextAccessor.HttpContext!.User.UserDisplayName();
        await _outerApiClient.PatchOrganisation(ukprn.ToString(), userDisplayName, patchDoc, cancellationToken);

        return true;
    }


    private static bool ChangeMade(PatchOrganisationModel newPatchModel, GetOrganisationResponse currentDetails)
    {
        return currentDetails.Status != newPatchModel.Status
               || currentDetails.OrganisationTypeId != newPatchModel.OrganisationTypeId
               || currentDetails.ProviderType != newPatchModel.ProviderType
               || currentDetails.RemovedReasonId != newPatchModel.RemovedReasonId;
    }
}
