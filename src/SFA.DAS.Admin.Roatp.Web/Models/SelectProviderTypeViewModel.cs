namespace SFA.DAS.Admin.Roatp.Web.Models;

public class SelectProviderTypeViewModel : IBackLink
{
    public List<AddProviderTypeSelectionModel> ProviderTypes { get; set; } = new();
    public int ProviderTypeId { get; set; }
}

public class SelectProviderTypeSubmitModel : SelectProviderTypeViewModel
{
}