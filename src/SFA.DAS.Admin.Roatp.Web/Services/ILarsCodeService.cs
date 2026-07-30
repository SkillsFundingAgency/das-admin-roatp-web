using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface ILarsCodeService
{
    Task<GetRestrictedCourseDetailsResponse?> GetCourseDetailsAsync(string larsCode, CancellationToken cancellationToken);
}
