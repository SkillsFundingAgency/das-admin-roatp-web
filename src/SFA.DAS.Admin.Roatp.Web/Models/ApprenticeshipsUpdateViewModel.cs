using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ApprenticeshipTypeUpdateViewModel : IBackLink
{
    public List<ApprenticeshipUnitsSelectionModel> ApprenticeshipUnitsSelection { get; set; } = new();
    public bool? ApprenticeshipUnitSelectionChoice { get; set; }
}