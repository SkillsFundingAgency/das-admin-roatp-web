using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class AllowedProviderViewModel
{
    public int Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public DateTime? LastDateStarts { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }

    public string DeliveryStatusDescription => DeliveryStatus.GetDescription();
    public string DeliveryStatusTagClass => DeliveryStatus.GetTagClass();
    public bool HasLastStartDate => LastDateStarts.HasValue;
    public string LastStartDateText => LastDateStarts.HasValue ? LastDateStarts.Value.ToScreenString() : string.Empty;
    public string ChangeUrl { get; set; } = "#";

    public static implicit operator AllowedProviderViewModel(ProviderCourseModel provider) => new()
    {
        Ukprn = provider.Ukprn,
        ProviderName = provider.ProviderName,
        LastDateStarts = provider.LastDateStarts,
        DeliveryStatus = provider.LastDateStarts.ToDeliveryStatus()
    };
}
