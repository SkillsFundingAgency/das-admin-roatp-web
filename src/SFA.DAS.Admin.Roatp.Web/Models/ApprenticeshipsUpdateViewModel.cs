using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ApprenticeshipsUpdateViewModel : IBackLink
{
    public List<ApprenticeshipsSelectionModel> ApprenticeshipsSelection { get; set; } = new();
    public bool? ApprenticeshipsSelectionChoice { get; set; }
}