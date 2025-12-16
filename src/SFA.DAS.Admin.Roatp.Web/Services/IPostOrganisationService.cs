using SFA.DAS.Admin.Roatp.Web.Models.Session;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public interface IPostOrganisationService
{
    Task PostOrganisation(AddProviderSessionModel addProviderSession, CancellationToken cancellationToken);
}