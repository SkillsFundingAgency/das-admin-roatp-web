using Refit;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;
public interface IOuterApiClient
{
    [Get("/ping")]
    Task<ApiResponse<string>> Ping(CancellationToken cancellationToken = default);
}
