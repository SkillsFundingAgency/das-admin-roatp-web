using Microsoft.AspNetCore.JsonPatch;
using Refit;
using SFA.DAS.Admin.Roatp.Application.Constants;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;
public interface IOuterApiClient
{
    [Get("/ping")]
    Task<ApiResponse<string>> Ping(CancellationToken cancellationToken = default);

    [Get("/organisations")]
    Task<GetOrganisationsResponse> GetOrganisations(CancellationToken cancellationToken);

    [Get("/organisations/{ukprn}")]
    Task<ApiResponse<GetOrganisationResponse>> GetOrganisation(int ukprn, CancellationToken cancellationToken);

    [Patch("/organisations/{ukprn}")]
    Task<ApiResponse<HttpStatusCode>> PatchOrganisation(int ukprn, [Header(RequestHeaders.RequestingUserIdHeader)] string userId, [Body] JsonPatchDocument<PatchOrganisationModel> patchDoc, CancellationToken cancellationToken);

    [Get("/removed-reasons")]
    Task<GetRemovalReasonsResponse> GetRemovalReasons(CancellationToken cancellationToken);

    [Get("/organisation-types")]
    Task<GetOrganisationTypesResponse> GetOrganisationTypes(CancellationToken cancellationToken);

    [Put("/organisations/{ukprn}/course-types")]
    Task PutCourseTypes(int ukprn, [Body] UpdateCourseTypesModel updateCourseTypesModel, CancellationToken cancellationToken);

    [Delete("/organisations/{ukprn}/short-courses")]
    Task DeleteShortCourses(int ukprn, [Header(RequestHeaders.RequestingUserIdHeader)] string userId, CancellationToken cancellationToken);

    [Get("/organisations/{ukprn}/ukrlp-data")]
    Task<ApiResponse<GetUkrlpResponse>> GetUkrlp(int ukprn, CancellationToken cancellationToken);
}
