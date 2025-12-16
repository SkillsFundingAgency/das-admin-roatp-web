using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class PostOrganisationService(IOuterApiClient _outerApiClient, IHttpContextAccessor _contextAccessor) : IPostOrganisationService
{
    public async Task PostOrganisation(AddProviderSessionModel addProviderSession, CancellationToken cancellationToken)
    {
        var command = new PostOrganisationCommand()
        {
            Ukprn = addProviderSession.Ukprn,
            LegalName = addProviderSession.LegalName,
            TradingName = addProviderSession.TradingName,
            CompanyNumber = addProviderSession.CompanyNumber,
            CharityNumber = addProviderSession.CharityNumber,
            ProviderType = (ProviderType)addProviderSession.ProviderTypeId!,
            OrganisationTypeId = addProviderSession.OrganisationTypeId ?? 0,
            RequestingUserId = _contextAccessor.HttpContext!.User.UserId(),
            RequestingUserDisplayName = _contextAccessor.HttpContext!.User.UserDisplayName(),
            DeliversApprenticeships = addProviderSession.OffersApprenticeships ?? false,
            DeliversApprenticeshipUnits = addProviderSession.OffersApprenticeshipUnits ?? false
        };

        await _outerApiClient.PostOrganisation(command, cancellationToken);
    }
}
