using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderStatusConfirmationViewModel
{
    public required string LegalName { get; set; }
    public required OrganisationStatus OrganisationStatus { get; set; }
    public required string StatusText { get; set; }
    public required int Ukprn { get; set; }
    public string ProviderSummaryLink { get; set; } = "#";
    public string SelectTrainingProviderLink { get; set; } = "#";
    public string AddNewTrainingProviderLink { get; set; } = "#";
}