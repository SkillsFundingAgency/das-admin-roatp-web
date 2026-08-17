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
    public void HasLearningTypeFilter_WhenAllTypesSelected_ThenIsFalse()
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

        request.HasLearningTypeFilter.Should().BeFalse();
        request.HasFilters.Should().BeFalse();
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
}
