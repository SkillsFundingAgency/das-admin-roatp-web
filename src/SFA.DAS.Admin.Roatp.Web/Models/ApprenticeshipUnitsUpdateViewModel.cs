using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ApprenticeshipUnitsUpdateViewModel : IBackLink
{
    public List<ApprenticeshipUnitsSelectionModel> ApprenticeshipUnitsSelection { get; set; } = new();
    public required bool OffersApprenticeships { get; set; }
    public bool? ApprenticeshipUnitsSelectionId { get; set; }
}