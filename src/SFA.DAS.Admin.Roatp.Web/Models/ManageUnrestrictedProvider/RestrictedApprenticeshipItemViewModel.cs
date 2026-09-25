using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class RestrictedApprenticeshipItemViewModel : ICourseDisplayModel
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public DateTime? LastDateStarts { get; set; }
    public bool IsClosedToNewStarts { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string ChangeUrl { get; set; } = string.Empty;

    public string DisplayTitle => this.GetDisplayTitle();
    public string DeliveryStatusDescription => DeliveryStatus.GetDescription();
    public string DeliveryStatusTagClass => DeliveryStatus.GetTagClass();

    public static implicit operator RestrictedApprenticeshipItemViewModel(RestrictedApprenticeshipModel course) => new()
    {
        LarsCode = course.LarsCode,
        Title = course.Title,
        Level = course.Level,
        LastDateStarts = course.LastDateStarts,
        IsClosedToNewStarts = course.IsClosedToNewStarts,
        DeliveryStatus = course.LastDateStarts.ToDeliveryStatus(course.IsClosedToNewStarts)
    };
}
