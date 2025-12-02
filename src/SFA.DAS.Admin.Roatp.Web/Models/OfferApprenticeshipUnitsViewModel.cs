using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class OfferApprenticeshipUnitsViewModel : OfferApprenticeshipUnitsSubmitModel, IBackLink
{
    public List<ApprenticeshipUnitsSelectionModel> ApprenticeshipUnitsSelection { get; set; } = new();
}

public class OfferApprenticeshipUnitsSubmitModel
{
    public bool? ApprenticeshipUnitsSelectionId { get; set; }
}