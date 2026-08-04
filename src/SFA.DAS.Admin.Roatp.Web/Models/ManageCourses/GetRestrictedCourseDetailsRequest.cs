using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class GetRestrictedCourseDetailsRequest
{
    public string SearchTerm { get; set; } = string.Empty;

    public List<DeliveryStatus> DeliveryStatus { get; set; } = [];

    public int PageNumber { get; set; } = 1;

    public bool HasSearchTermFilter => !string.IsNullOrWhiteSpace(SearchTerm);

    public bool HasDeliveryStatusFilter => DeliveryStatus.Count > 0;

    public bool HasFilters => HasSearchTermFilter || HasDeliveryStatusFilter;

    public List<(string, string)> ToQueryString()
    {
        var queryParams = new List<(string, string)>();

        if (HasSearchTermFilter)
        {
            queryParams.Add((nameof(SearchTerm), SearchTerm.Trim()));
        }

        foreach (var status in DeliveryStatus.Distinct())
        {
            queryParams.Add((nameof(DeliveryStatus), status.ToString()));
        }

        return queryParams;
    }
}
