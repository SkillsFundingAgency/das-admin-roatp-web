using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class AddProviderTypeSelectionModel : RadioButtonSelectionModel
{
    public required int Id { get; set; }
    public required string Description { get; set; }
}
