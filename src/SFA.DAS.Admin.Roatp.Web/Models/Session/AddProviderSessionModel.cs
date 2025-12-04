namespace SFA.DAS.Admin.Roatp.Web.Models.Session;
public class AddProviderSessionModel : ISessionModel
{
    public int Ukprn { get; set; }
    public required string LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? CharityNumber { get; set; }
    public string? CompanyNumber { get; set; }
    public int? ProviderTypeId { get; set; }
    public bool? OffersApprenticeships { get; set; }
    public bool? OffersApprenticeshipUnits { get; set; }
    public int? OrganisationTypeId { get; set; }
    public void ResetModel()
    {
        OffersApprenticeships = null;
        OffersApprenticeshipUnits = null;
        OrganisationTypeId = null;
    }
}