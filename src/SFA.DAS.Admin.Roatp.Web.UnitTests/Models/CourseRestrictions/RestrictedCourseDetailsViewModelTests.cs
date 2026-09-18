using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.CourseRestrictions;

[TestFixture]
public class RestrictedCourseDetailsViewModelTests
{
    [Test]
    public void WhenIsCourseRestrictedIsTrue_ThenStatusTextIsRestricted()
    {
        var model = CreateViewModel(isCourseRestricted: true);

        model.StatusText.Should().Be("Restricted");
    }

    [Test]
    public void WhenIsCourseRestrictedIsFalse_ThenStatusTextIsUnrestricted()
    {
        var model = CreateViewModel(isCourseRestricted: false);

        model.StatusText.Should().Be("Unrestricted");
    }

    [Test]
    public void WhenFiltersAppliedWithNoMatches_ThenShowsNoFilterResults()
    {
        var model = CreateViewModel();
        model.HasActiveFilters = true;
        model.AllowedProviders = [];

        using (new AssertionScope())
        {
            model.HasNoFilteredResults.Should().BeTrue();
            model.HasNoProviders.Should().BeFalse();
            model.ShowProviderResults.Should().BeTrue();
        }
    }

    [Test]
    public void WhenSuccessBannerMessageIsSet_ThenHasSuccessBannerIsTrue()
    {
        var model = CreateViewModel();
        model.SuccessBannerMessage = "Last start date added for BP TRAINING";

        model.HasSuccessBanner.Should().BeTrue();
    }

    [Test]
    public void WhenSuccessBannerMessageIsBlank_ThenHasSuccessBannerIsFalse()
    {
        var model = CreateViewModel();
        model.SuccessBannerMessage = " ";

        model.HasSuccessBanner.Should().BeFalse();
    }

    [Test]
    public void BackLinkText_ReturnsExpectedText()
    {
        var model = CreateViewModel();

        using (new AssertionScope())
        {
            model.RestrictedCourseDetailsPageUrl.Should().Be("#");
            model.HasSuccessBanner.Should().BeFalse();
        }
    }

    private static RestrictedCourseDetailsViewModel CreateViewModel(bool isCourseRestricted = true)
        => new()
        {
            LarsCode = "124",
            CourseName = "Course",
            Title = "Course",
            Sector = "Sector",
            IsCourseRestricted = isCourseRestricted
        };
}
