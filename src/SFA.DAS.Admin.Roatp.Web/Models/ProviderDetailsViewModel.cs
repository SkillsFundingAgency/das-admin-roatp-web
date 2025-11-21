namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderDetailsViewModel : IBackLink
{
    public int Ukprn { get; set; }
    public string? LegalName { get; set; } = string.Empty;
    public string? TradingName { get; set; } = string.Empty;
    public string? CharityNumber { get; set; } = string.Empty;
    public string? CompanyNumber { get; set; } = string.Empty;
    public string AddProviderRouteUrl { get; set; } = "#";
    public string SelectProviderUrl { get; set; } = "#";
}