using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class GetRestrictedCourseDetailsRequest
{
    [FromQuery]
    public string? ProviderName { get; set; }

    [FromQuery]
    public List<DeliveryStatus> DeliveryStatus { get; set; } = [];

    public bool HasProviderNameFilter => !string.IsNullOrWhiteSpace(ProviderName);

    public bool HasDeliveryStatusFilter => DeliveryStatus.Count > 0;

    public bool HasFilters => HasProviderNameFilter || HasDeliveryStatusFilter;
}
