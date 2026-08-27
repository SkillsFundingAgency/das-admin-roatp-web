using System.Security.Claims;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IRestrictedCourseProviderService
{
    Task<bool> IsRouteValidAsync(string larsCode, int ukprn, CancellationToken cancellationToken);

    Task<(GetRestrictedCourseDetailsResponse Course, ProviderCourseModel Provider)?> GetCourseAndProviderAsync(
        string larsCode,
        int ukprn,
        CancellationToken cancellationToken);

    UpsertProviderAllowedCourseRequest CreateUpsertRequest(ClaimsPrincipal user, DateTime? lastDateStarts);
}
