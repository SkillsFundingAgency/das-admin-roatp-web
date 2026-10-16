namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class ConfirmRestrictProviderViewModel
{
    public int Ukprn { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = "#";
}
