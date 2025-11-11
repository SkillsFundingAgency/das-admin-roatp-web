using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class OrganisationPatchService(IOuterApiClient _outerApiClient, IHttpContextAccessor _contextAccessor) : IOrganisationPatchService
{
    public async Task<bool> OrganisationPatched(int ukprn, GetOrganisationResponse organisationResponse, PatchOrganisationModel patchModel, CancellationToken cancellationToken)
    {
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

        if (!ChangeMade(patchDoc)) return false;

        string userDisplayName = _contextAccessor.HttpContext!.User.UserDisplayName();

        var response = await _outerApiClient.PatchOrganisation(ukprn.ToString(), userDisplayName, patchDoc, cancellationToken);

        return response.StatusCode == HttpStatusCode.NoContent;
    }

    private static bool ChangeMade(JsonPatchDocument<PatchOrganisationModel> model)
    {
        return model.Operations.Count > 0;
    }
}
