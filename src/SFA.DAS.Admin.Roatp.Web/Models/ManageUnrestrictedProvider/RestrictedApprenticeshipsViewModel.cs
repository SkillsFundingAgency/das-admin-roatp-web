using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class RestrictedApprenticeshipsViewModel : ICustomBackLink
{
    public const string BackLinkTextValue = "Back to organisation details";

    public string ProviderName { get; set; } = string.Empty;
    public string RestrictACourseUrl { get; set; } = "#";
    public string BackLinkUrl { get; set; } = "#";
    public string BackLinkText => BackLinkTextValue;
    public IReadOnlyList<RestrictedApprenticeshipItemViewModel> Courses { get; set; } = [];

    public int TotalCount => Courses.Count;
    public bool HasCourses => TotalCount > 0;
    public bool HasNoCourses => !HasCourses;
    public string TotalCountDescription => "course".ToQuantity(TotalCount);

    public static implicit operator RestrictedApprenticeshipsViewModel(GetRestrictedApprenticeshipsResponse? response)
    {
        var courses = response?.Courses ?? [];

        return new RestrictedApprenticeshipsViewModel
        {
            Courses = courses
                .Select(course => (RestrictedApprenticeshipItemViewModel)course)
                .Where(course => course.DeliveryStatus != DeliveryStatus.OpenToNewStarts)
                .OrderBy(course => course.DisplayTitle, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }
}
