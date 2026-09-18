using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.CourseRestrictions;

[TestFixture]
public class GetRestrictedCoursesModelTests
{
    [Test]
    public void HasLearningTypeFilter_WhenNoTypesSelected_ThenIsFalse()
    {
        var model = new GetRestrictedCoursesModel();

        model.HasLearningTypeFilter.Should().BeFalse();
        model.HasFilters.Should().BeFalse();
    }

    [Test]
    public void HasLearningTypeFilter_WhenSomeTypesSelected_ThenIsTrue()
    {
        var model = new GetRestrictedCoursesModel
        {
            LearningType = [LearningType.Apprenticeship]
        };

        model.HasLearningTypeFilter.Should().BeTrue();
        model.HasFilters.Should().BeTrue();
    }

    [Test]
    public void HasLearningTypeFilter_WhenAllTypesSelected_ThenIsTrue()
    {
        var model = new GetRestrictedCoursesModel
        {
            LearningType =
            [
                LearningType.Apprenticeship,
                LearningType.ApprenticeshipUnit,
                LearningType.FoundationApprenticeship
            ]
        };

        model.HasLearningTypeFilter.Should().BeTrue();
        model.HasFilters.Should().BeTrue();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermProvided_ThenIsTrue()
    {
        var model = new GetRestrictedCoursesModel
        {
            SearchTerm = "Paint"
        };

        model.HasSearchTermFilter.Should().BeTrue();
        model.HasFilters.Should().BeTrue();
    }

    [Test]
    public void ToQueryString_WhenFiltersApplied_ThenIncludesSearchTermAndLearningTypes()
    {
        var model = new GetRestrictedCoursesModel
        {
            SearchTerm = " Paint ",
            LearningType = [LearningType.Apprenticeship, LearningType.ApprenticeshipUnit]
        };

        var queryString = model.ToQueryString();

        queryString.Should().Contain((nameof(GetRestrictedCoursesModel.SearchTerm), "Paint"));
        queryString.Should().Contain((nameof(GetRestrictedCoursesModel.LearningType), nameof(LearningType.Apprenticeship)));
        queryString.Should().Contain((nameof(GetRestrictedCoursesModel.LearningType), nameof(LearningType.ApprenticeshipUnit)));
        queryString.Should().NotContain(q => q.Item1 == nameof(GetRestrictedCoursesModel.PageNumber));
    }

    [Test]
    public void ToQueryString_WhenNoFilters_ThenReturnsEmpty()
    {
        var model = new GetRestrictedCoursesModel();

        model.ToQueryString().Should().BeEmpty();
    }
}
