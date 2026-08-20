using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class GetRestrictedCoursesRequestTests
{
    [Test]
    public void HasLearningTypeFilter_WhenNoTypesSelected_ThenIsFalse()
    {
        var request = new GetRestrictedCoursesRequest();

        request.HasLearningTypeFilter.Should().BeFalse();
        request.HasFilters.Should().BeFalse();
    }

    [Test]
    public void HasLearningTypeFilter_WhenSomeTypesSelected_ThenIsTrue()
    {
        var request = new GetRestrictedCoursesRequest
        {
            LearningType = [LearningType.Apprenticeship]
        };

        request.HasLearningTypeFilter.Should().BeTrue();
        request.HasFilters.Should().BeTrue();
    }

    [Test]
    public void HasLearningTypeFilter_WhenAllTypesSelected_ThenIsTrue()
    {
        var request = new GetRestrictedCoursesRequest
        {
            LearningType =
            [
                LearningType.Apprenticeship,
                LearningType.ApprenticeshipUnit,
                LearningType.FoundationApprenticeship
            ]
        };

        request.HasLearningTypeFilter.Should().BeTrue();
        request.HasFilters.Should().BeTrue();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermProvided_ThenIsTrue()
    {
        var request = new GetRestrictedCoursesRequest
        {
            SearchTerm = "Paint"
        };

        request.HasSearchTermFilter.Should().BeTrue();
        request.HasFilters.Should().BeTrue();
    }

    [Test]
    public void ToQueryString_WhenFiltersApplied_ThenIncludesSearchTermAndLearningTypes()
    {
        var request = new GetRestrictedCoursesRequest
        {
            SearchTerm = " Paint ",
            LearningType = [LearningType.Apprenticeship, LearningType.ApprenticeshipUnit]
        };

        var queryString = request.ToQueryString();

        queryString.Should().Contain((nameof(GetRestrictedCoursesRequest.SearchTerm), "Paint"));
        queryString.Should().Contain((nameof(GetRestrictedCoursesRequest.LearningType), nameof(LearningType.Apprenticeship)));
        queryString.Should().Contain((nameof(GetRestrictedCoursesRequest.LearningType), nameof(LearningType.ApprenticeshipUnit)));
        queryString.Should().NotContain(q => q.Item1 == nameof(GetRestrictedCoursesRequest.PageNumber));
    }

    [Test]
    public void ToQueryString_WhenNoFilters_ThenReturnsEmpty()
    {
        var request = new GetRestrictedCoursesRequest();

        request.ToQueryString().Should().BeEmpty();
    }
}
