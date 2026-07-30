using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class GetRestrictedCourseDetailsRequest
{
    [FromQuery]
    public string SearchTerm { get; set; } = string.Empty;

    [FromQuery]
    public List<DeliveryStatus> DeliveryStatus { get; set; } = [];

    public bool HasSearchTermFilter => !string.IsNullOrWhiteSpace(SearchTerm);

    public bool HasDeliveryStatusFilter => DeliveryStatus.Count > 0;

    public bool HasFilters => HasSearchTermFilter || HasDeliveryStatusFilter;
}
