using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderTypeUpdateViewModel : IBackLink
{
    public List<ProviderTypeSelectionModel> ProviderTypes { get; set; } = new();
    public required int ProviderTypeId { get; set; }

}