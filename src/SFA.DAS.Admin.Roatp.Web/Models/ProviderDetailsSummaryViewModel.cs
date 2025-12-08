using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderDetailsSummaryViewModel : IBackLink
{
    public int Ukprn { get; set; }
    public string? LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? ProviderRoute { get; set; }
    public string? OffersApprenticeshipsText { get; set; }
    public string? OffersApprenticeshipUnitsText { get; set; }
    public string? OrganisationType { get; set; }
    public bool IsSupportingProvider { get; set; }
    public string ProviderTypeChangeLink { get; set; } = "#";
    public string OffersApprenticeshipsChangeLink { get; set; } = "#";
    public string OffersApprenticeshipUnitsChangeLink { get; set; } = "#";
    public string OrganisationTypeChangeLink { get; set; } = "#";
    public string ManageProviderLink { get; set; } = "#";
    public static implicit operator ProviderDetailsSummaryViewModel(AddProviderSessionModel sessionModel) => new ProviderDetailsSummaryViewModel
    {
        Ukprn = sessionModel.Ukprn,
        LegalName = sessionModel.LegalName,
        TradingName = sessionModel.TradingName ?? "Not applicable",
        ProviderRoute = ((ProviderType)sessionModel.ProviderTypeId!).ToString(),
        OffersApprenticeshipsText = sessionModel.OffersApprenticeships == true ? "Yes" : "No",
        OffersApprenticeshipUnitsText = sessionModel.OffersApprenticeshipUnits == true ? "Yes" : "No",
        OrganisationType = sessionModel.OrganisationType,
        IsSupportingProvider = sessionModel.ProviderTypeId == (int)ProviderType.Supporting
    };
}
