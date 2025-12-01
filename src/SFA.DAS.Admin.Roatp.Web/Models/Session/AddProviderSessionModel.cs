namespace SFA.DAS.Admin.Roatp.Web.Models.Session;
public class AddProviderSessionModel
{
    public int Ukprn { get; set; }
    public required string LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? CharityNumber { get; set; }
    public string? CompanyNumber { get; set; }
    public int? ProviderTypeId { get; set; }
    public bool? OfferApprenticeships { get; set; }
    public void ClearSessionProperty(string propertyName)
    {
        var propertyType = GetType().GetProperty(propertyName);
        propertyType?.SetValue(this, default);
    }
}