namespace SFA.DAS.Admin.Roatp.Domain.Models;
public class AddProviderSessionModel
{
    public int Ukprn { get; set; }
    public string? LegalName { get; set; } = string.Empty;
    public string? TradingName { get; set; } = string.Empty;
    public string? CharityNumber { get; set; } = string.Empty;
    public string? CompanyNumber { get; set; } = string.Empty;
}