using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class OfferApprenticeshipsViewModel : OfferApprenticeshipsSubmitModel, IBackLink
{
    public List<ApprenticeshipsSelectionModel> ApprenticeshipsSelection { get; set; } = new();
}

public class OfferApprenticeshipsSubmitModel
{
    public bool? ApprenticeshipsSelectionChoice { get; set; }
}