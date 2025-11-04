using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class RemovalReasonUpdateViewModel : IBackLink
{
    public List<RemovalReasonModel> RemovedReasons { get; set; } = [];
    public int? RemovalReasonId { get; set; }
}