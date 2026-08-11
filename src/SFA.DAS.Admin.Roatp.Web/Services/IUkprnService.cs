using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IUkprnService
{
    Task<GetOrganisationResponse?> GetOrganisationAsync(int ukprn, CancellationToken cancellationToken);
}
