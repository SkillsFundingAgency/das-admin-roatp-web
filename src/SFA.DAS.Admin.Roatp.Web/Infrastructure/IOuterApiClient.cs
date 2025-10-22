using Refit;
using SFA.DAS.Admin.Roatp.Web.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;
public interface IOuterApiClient
{
    [Get("/ping")]
    Task<ApiResponse<string>> Ping(CancellationToken cancellationToken = default);

    [Get("/organisations")]
    Task<GetOrganisationsResponse> GetOrganisations(CancellationToken cancellationToken);

}
