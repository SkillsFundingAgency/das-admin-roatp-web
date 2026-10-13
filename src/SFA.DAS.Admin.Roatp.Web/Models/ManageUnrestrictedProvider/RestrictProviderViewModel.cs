namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class RestrictProviderViewModel : ICustomBackLink
{
    public const string BackLinkTextValue = "Back to organisation details";

    public int Ukprn { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = "#";
    public string BackLinkUrl { get; set; } = "#";
    public string BackLinkText => BackLinkTextValue;
}
