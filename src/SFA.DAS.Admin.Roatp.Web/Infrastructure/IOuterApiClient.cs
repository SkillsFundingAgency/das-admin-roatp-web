using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using Refit;
using SFA.DAS.Admin.Roatp.Application.Constants;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;
public interface IOuterApiClient
{
    [Get("/ping")]
    Task<ApiResponse<string>> Ping(CancellationToken cancellationToken = default);

    [Get("/organisations")]
    Task<GetOrganisationsResponse> GetOrganisations(CancellationToken cancellationToken);

    [Get("/organisations/{ukprn}")]
    Task<GetOrganisationResponse> GetOrganisation(string ukprn, CancellationToken cancellationToken);

    [Patch("/organisations/{ukprn}")]
    Task<ApiResponse<HttpStatusCode>> PatchOrganisation(string ukprn, [Header(RequestHeaders.RequestingUserIdHeader)] string userId, [Body] JsonPatchDocument<PatchOrganisationModel> patchDoc, CancellationToken cancellationToken);

    [Get("/removed-reasons")]
    Task<GetRemovalReasonsResponse> GetRemovalReasons(CancellationToken cancellationToken);

    [Get("/organisation-types")]
    Task<GetOrganisationTypesResponse> GetOrganisationTypes(CancellationToken cancellationToken);
}
