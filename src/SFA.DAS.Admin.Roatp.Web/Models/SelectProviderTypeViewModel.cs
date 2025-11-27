namespace SFA.DAS.Admin.Roatp.Web.Models;

public class SelectProviderTypeViewModel : SelectProviderTypeSubmitModel, IBackLink
{
}

public class SelectProviderTypeSubmitModel
{
    public List<AddProviderTypeSelectionModel> ProviderTypes { get; set; } = new();
    public int? ProviderTypeId { get; set; }
}