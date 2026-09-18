using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.CourseRestrictions;

[TestFixture]
public class GetRestrictedCoursesRequestModelTests
{
    [Test]
    public void HasLearningTypeFilter_WhenNoTypesSelected_ThenIsFalse()
    {
        var requestModel = new GetRestrictedCoursesRequestModel();

        requestModel.HasLearningTypeFilter.Should().BeFalse();
        requestModel.HasFilters.Should().BeFalse();
    }

    [Test]
    public void HasLearningTypeFilter_WhenSomeTypesSelected_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            LearningType = [LearningType.Apprenticeship]
        };

        requestModel.HasLearningTypeFilter.Should().BeTrue();
        requestModel.HasFilters.Should().BeTrue();
    }

    [Test]
    public void HasLearningTypeFilter_WhenAllTypesSelected_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            LearningType =
            [
                LearningType.Apprenticeship,
                LearningType.ApprenticeshipUnit,
                LearningType.FoundationApprenticeship
            ]
        };

        requestModel.HasLearningTypeFilter.Should().BeTrue();
        requestModel.HasFilters.Should().BeTrue();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermProvided_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            SearchTerm = "Paint"
        };

        requestModel.HasSearchTermFilter.Should().BeTrue();
        requestModel.HasFilters.Should().BeTrue();
    }

    [Test]
    public void ToQueryString_WhenFiltersApplied_ThenIncludesSearchTermAndLearningTypes()
    {
        var requestModel = new GetRestrictedCoursesRequestModel
        {
            SearchTerm = " Paint ",
            LearningType = [LearningType.Apprenticeship, LearningType.ApprenticeshipUnit]
        };

        var queryString = requestModel.ToQueryString();

        queryString.Should().Contain((nameof(GetRestrictedCoursesRequestModel.SearchTerm), "Paint"));
        queryString.Should().Contain((nameof(GetRestrictedCoursesRequestModel.LearningType), nameof(LearningType.Apprenticeship)));
        queryString.Should().Contain((nameof(GetRestrictedCoursesRequestModel.LearningType), nameof(LearningType.ApprenticeshipUnit)));
        queryString.Should().NotContain(q => q.Item1 == nameof(GetRestrictedCoursesRequestModel.PageNumber));
    }

    [Test]
    public void ToQueryString_WhenNoFilters_ThenReturnsEmpty()
    {
        var requestModel = new GetRestrictedCoursesRequestModel();

        requestModel.ToQueryString().Should().BeEmpty();
    }
}
