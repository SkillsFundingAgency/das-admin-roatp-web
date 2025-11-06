using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderTypeUpdateViewModel : IBackLink
{
    public List<OrganisationRouteSelectionModel> ProviderTypes { get; set; } = new();
    public required int ProviderTypeId { get; set; }

}