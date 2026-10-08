using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public class AllowedCoursesViewModel : ICustomBackLink
{
    public const string BackLinkTextValue = "Back to organisation details";

    public CourseType CourseType { get; set; }
    public AllowedCoursesContent PageContent { get; set; } = AllowedCoursesContent.CreateForCourseType(CourseType.Apprenticeship);
    public string ProviderName { get; set; } = string.Empty;
    public string AddUrl { get; set; } = "#";
    public string BackLinkUrl { get; set; } = "#";
    public string BackLinkText => BackLinkTextValue;
    public IReadOnlyList<AllowedCourseItemViewModel> Courses { get; set; } = [];

    public int TotalCount { get; set; }
    public bool HasCourses => TotalCount > 0;
    public bool HasNoCourses => !HasCourses;
    public string TotalCountDescription => "course".ToQuantity(TotalCount);
}
