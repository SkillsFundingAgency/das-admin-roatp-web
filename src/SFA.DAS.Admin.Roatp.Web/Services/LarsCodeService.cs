using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class LarsCodeService(IOuterApiClient outerApiClient) : ILarsCodeService
{
    private readonly Dictionary<string, GetRestrictedCourseDetailsResponse?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(string larsCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(larsCode))
        {
            return null;
        }

        if (_cache.TryGetValue(larsCode, out var cached))
        {
            return cached;
        }

        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);

        var courseDetails = response.IsSuccessStatusCode ? response.Content : null;
        _cache[larsCode] = courseDetails;

        return courseDetails;
    }
}
