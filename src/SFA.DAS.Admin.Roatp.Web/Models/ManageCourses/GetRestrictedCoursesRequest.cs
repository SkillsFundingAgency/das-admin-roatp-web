using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class GetRestrictedCoursesRequest
{
    public string SearchTerm { get; set; } = string.Empty;

    public List<LearningType> LearningType { get; set; } = [];

    public bool HasSearchTermFilter => !string.IsNullOrWhiteSpace(SearchTerm);

    public bool HasLearningTypeFilter =>
        LearningType.Count > 0
        && LearningType.Distinct().Count() < Enum.GetValues<LearningType>().Length;

    public bool HasFilters => HasSearchTermFilter || HasLearningTypeFilter;
}
