namespace SFA.DAS.Admin.Roatp.Web.Models;

public class SelectProviderTypeViewModel : SelectProviderTypeSubmitModel, IBackLink
{
    public List<AddProviderTypeSelectionModel> ProviderTypes { get; set; } = new();
}

public class SelectProviderTypeSubmitModel
{
    public int? SelectedProviderTypeId { get; set; }
}