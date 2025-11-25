namespace SFA.DAS.Admin.Roatp.Web.Models.Session;
public class AddProviderSessionModel
{
    public int Ukprn { get; set; }
    public required string LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? CharityNumber { get; set; }
    public string? CompanyNumber { get; set; }
}