using Microsoft.AspNetCore.JsonPatch;
using Refit;
using SFA.DAS.Admin.Roatp.Application.Constants;
using SFA.DAS.Admin.Roatp.Domain.Models;
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
    Task<ApiResponse<GetOrganisationResponse>> GetOrganisation(int ukprn, CancellationToken cancellationToken);

    [Patch("/organisations/{ukprn}")]
    Task PatchOrganisation(int ukprn, [Header(RequestHeaders.RequestingUserIdHeader)] string userId, [Header(RequestHeaders.RequestingUserNameHeader)] string userName, [Body] JsonPatchDocument<PatchOrganisationModel> patchDoc, CancellationToken cancellationToken);

    [Get("/removed-reasons")]
    Task<GetRemovalReasonsResponse> GetRemovalReasons(CancellationToken cancellationToken);

    [Get("/organisation-types")]
    Task<GetOrganisationTypesResponse> GetOrganisationTypes(CancellationToken cancellationToken);

    [Put("/organisations/{ukprn}/course-types")]
    Task PutCourseTypes(int ukprn, [Body] UpdateCourseTypesModel updateCourseTypesModel, CancellationToken cancellationToken);

    [Get("/organisations/{ukprn}/ukrlp-data")]
    Task<ApiResponse<GetUkrlpResponse>> GetUkrlp(int ukprn, CancellationToken cancellationToken);

    [Post("/organisations")]
    Task PostOrganisation(PostOrganisationCommand command, CancellationToken cancellationToken);

    [Get("/restricted-courses")]
    Task<GetRestrictedCoursesResponse> GetRestrictedCourses(
        [Query] bool restricted,
        CancellationToken cancellationToken);

    [Get("/courses/{larsCode}/providers/allowed")]
    Task<ApiResponse<GetRestrictedCourseDetailsResponse>> GetAllowedProvidersForCourse(
        string larsCode,
        CancellationToken cancellationToken);

    [Patch("/providers/{ukprn}/allowed-courses/{larsCode}")]
    Task<ApiResponse<object>> PatchProviderAllowedCourse(
        int ukprn,
        string larsCode,
        [Header(RequestHeaders.RequestingUserIdHeader)] string userId,
        [Header(RequestHeaders.RequestingUserNameHeader)] string userName,
        [Body] PatchProviderAllowedCourseRequest request,
        CancellationToken cancellationToken);

    [Post("/providers/{ukprn}/allowed-courses/{larsCode}")]
    Task<ApiResponse<object>> UpsertProviderAllowedCourse(
        int ukprn,
        string larsCode,
        [Body] UpsertProviderAllowedCourseRequest request,
        CancellationToken cancellationToken);

    [Post("/restricted-courses")]
    Task AddRestrictedCourse(
        [Body] AddRestrictedCourseRequest request,
        CancellationToken cancellationToken);

    [Get("/courses/{larsCode}/providers/not-allowed")]
    Task<ApiResponse<GetRestrictedCourseDetailsResponse>> GetProvidersRestrictedForCourse(
        string larsCode,
        CancellationToken cancellationToken);

    [Get("/providers/{ukprn}/restricted-apprenticeships")]
    Task<ApiResponse<GetRestrictedApprenticeshipsResponse>> GetRestrictedApprenticeships(
        int ukprn,
        CancellationToken cancellationToken);

    [Post("/providers/{ukprn}/restricted-apprenticeships/{larsCode}/change")]
    Task<ApiResponse<object>> ChangeRestrictedApprenticeshipLastDateStarts(
        int ukprn,
        string larsCode,
        [Body] ChangeRestrictedApprenticeshipLastDateStartsRequest request,
        CancellationToken cancellationToken);

    [Post("/providers/{ukprn}/restricted-apprenticeships/{larsCode}/remove")]
    Task<ApiResponse<object>> RemoveRestrictedApprenticeship(
        int ukprn,
        string larsCode,
        [Body] RemoveRestrictedApprenticeshipRequest request,
        CancellationToken cancellationToken);

    [Post("/providers/{ukprn}/restricted-apprenticeships/{larsCode}/add")]
    Task<ApiResponse<object>> AddRestrictedApprenticeship(
        int ukprn,
        string larsCode,
        [Body] AddRestrictedApprenticeshipRequest request,
        CancellationToken cancellationToken);

    [Get("/providers/{ukprn}/not-restricted-apprenticeships")]
    Task<ApiResponse<GetNotRestrictedApprenticeshipsResponse>> GetNotRestrictedApprenticeships(
        int ukprn,
        CancellationToken cancellationToken);

    [Get("/providers/{ukprn}/courses/{larsCode}")]
    Task<ApiResponse<object>> GetProviderCourse(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken);

    [Get("/courses")]
    Task<ApiResponse<GetCoursesResponse>> GetCourses(CancellationToken cancellationToken);

    [Get("/providers/{ukprn}/allowed-courses")]
    Task<ApiResponse<GetAllowedCoursesResponse>> GetAllowedCourses(
    int ukprn,
    [Query] CourseType courseType,
    CancellationToken cancellationToken);
}
