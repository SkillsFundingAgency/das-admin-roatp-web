using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ApprenticeshipUnitUpdateViewModel : IBackLink
{
    public List<ApprenticeshipUnitsSelectionModel> ApprenticeshipUnitsSelection { get; set; } = new();
    public required bool ApprenticeshipUnitSelectionChoice { get; set; }
}