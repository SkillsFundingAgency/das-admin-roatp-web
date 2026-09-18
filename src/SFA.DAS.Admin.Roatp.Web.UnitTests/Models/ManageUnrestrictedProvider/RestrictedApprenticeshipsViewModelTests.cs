using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageUnrestrictedProvider;

[TestFixture]
public class RestrictedApprenticeshipsViewModelTests
{
    [Test]
    public void WhenTotalCountIsOne_ThenTotalCountDescriptionIsSingular()
    {
        var model = new RestrictedApprenticeshipsViewModel
        {
            TotalCount = 1
        };

        model.TotalCountDescription.Should().Be("1 course");
    }

    [Test]
    public void WhenTotalCountIsMultiple_ThenTotalCountDescriptionIsPlural()
    {
        var model = new RestrictedApprenticeshipsViewModel
        {
            TotalCount = 2
        };

        model.TotalCountDescription.Should().Be("2 courses");
    }

    [Test]
    public void WhenNoActiveFiltersAndNoCourses_ThenHasNoFilteredResultsIsFalse()
    {
        var model = new RestrictedApprenticeshipsViewModel();

        using (new AssertionScope())
        {
            model.HasNoFilteredResults.Should().BeFalse();
            model.HasNoCourses.Should().BeTrue();
            model.ShowCourseResults.Should().BeFalse();
            model.HasCourses.Should().BeFalse();
        }
    }

    [Test]
    public void WhenActiveFiltersAndNoCourses_ThenShowsNoFilterResults()
    {
        var model = new RestrictedApprenticeshipsViewModel
        {
            HasActiveFilters = true
        };

        using (new AssertionScope())
        {
            model.HasNoFilteredResults.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
            model.ShowCourseResults.Should().BeTrue();
            model.HasCourses.Should().BeFalse();
        }
    }

    [Test]
    public void WhenActiveFiltersAndHasCourses_ThenHasNoFilteredResultsIsFalse()
    {
        var model = new RestrictedApprenticeshipsViewModel
        {
            HasActiveFilters = true,
            TotalCount = 1
        };

        using (new AssertionScope())
        {
            model.HasNoFilteredResults.Should().BeFalse();
            model.HasNoCourses.Should().BeFalse();
            model.ShowCourseResults.Should().BeTrue();
            model.HasCourses.Should().BeTrue();
        }
    }
}
