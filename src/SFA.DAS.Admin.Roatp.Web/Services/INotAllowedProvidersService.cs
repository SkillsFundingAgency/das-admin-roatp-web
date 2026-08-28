using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface INotAllowedProvidersService
{
    Task<GetRestrictedCourseDetailsResponse?> GetNotAllowedProvidersAsync(
        string larsCode,
        CancellationToken cancellationToken);
}
