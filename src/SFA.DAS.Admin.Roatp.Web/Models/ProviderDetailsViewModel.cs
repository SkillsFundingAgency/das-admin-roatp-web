namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderDetailsViewModel : IBackLink
{
    public int Ukprn { get; set; }
    public required string LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? CharityNumber { get; set; }
    public string? CompanyNumber { get; set; }
    public string SelectProviderUrl { get; set; } = "#";
}