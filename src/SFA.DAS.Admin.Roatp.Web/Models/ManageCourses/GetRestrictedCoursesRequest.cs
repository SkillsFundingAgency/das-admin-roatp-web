using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class GetRestrictedCoursesRequest
{
    public string SearchTerm { get; set; } = string.Empty;

    public List<LearningType> LearningType { get; set; } = [];

    public int PageNumber { get; set; } = 1;

    public bool HasSearchTermFilter => !string.IsNullOrWhiteSpace(SearchTerm);

    public bool HasLearningTypeFilter => LearningType.Count > 0;

    public bool HasFilters => HasSearchTermFilter || HasLearningTypeFilter;

    public List<(string, string)> ToQueryString()
    {
        var queryParams = new List<(string, string)>();

        if (HasSearchTermFilter)
        {
            queryParams.Add((nameof(SearchTerm), SearchTerm.Trim()));
        }

        foreach (var learningType in LearningType.Distinct())
        {
            queryParams.Add((nameof(LearningType), learningType.ToString()));
        }

        return queryParams;
    }
}
