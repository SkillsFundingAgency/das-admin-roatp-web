using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageRestrictedProvider;

[TestFixture]
public class AllowedCoursesViewModelTests
{
    [Test]
    public void WhenTotalCountIsOne_ThenTotalCountDescriptionIsSingular()
    {
        var model = new AllowedCoursesViewModel { TotalCount = 1 };

        model.TotalCountDescription.Should().Be("1 course");
    }

    [Test]
    public void WhenTotalCountIsMultiple_ThenTotalCountDescriptionIsPlural()
    {
        var model = new AllowedCoursesViewModel { TotalCount = 20 };

        model.TotalCountDescription.Should().Be("20 courses");
    }

    [Test]
    public void WhenTotalCountIsZero_ThenHasNoCoursesIsTrue()
    {
        var model = new AllowedCoursesViewModel();

        using (new AssertionScope())
        {
            model.HasNoCourses.Should().BeTrue();
            model.HasCourses.Should().BeFalse();
        }
    }

    [Test]
    public void WhenTotalCountIsGreaterThanZero_ThenHasCoursesIsTrue()
    {
        var model = new AllowedCoursesViewModel { TotalCount = 1 };

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
        }
    }

    [Test]
    public void WhenRouteLearningTypeIsApprenticeship_ThenIsApprenticeshipIsTrue()
    {
        var model = new AllowedCoursesViewModel { RouteLearningType = RouteLearningType.Apprenticeship };

        using (new AssertionScope())
        {
            model.IsApprenticeship.Should().BeTrue();
            model.IsApprenticeshipUnit.Should().BeFalse();
        }
    }

    [Test]
    public void WhenRouteLearningTypeIsApprenticeshipUnit_ThenIsApprenticeshipUnitIsTrue()
    {
        var model = new AllowedCoursesViewModel { RouteLearningType = RouteLearningType.ApprenticeshipUnit };

        using (new AssertionScope())
        {
            model.IsApprenticeshipUnit.Should().BeTrue();
            model.IsApprenticeship.Should().BeFalse();
        }
    }

    [Test]
    public void WhenSuccessBannerMessageIsSet_ThenHasSuccessBannerIsTrue()
    {
        var model = new AllowedCoursesViewModel
        {
            SuccessBannerMessage = "This training provider can now only deliver approved courses"
        };

        model.HasSuccessBanner.Should().BeTrue();
    }

    [Test]
    public void WhenSuccessBannerMessageIsBlank_ThenHasSuccessBannerIsFalse()
    {
        var model = new AllowedCoursesViewModel();

        model.HasSuccessBanner.Should().BeFalse();
    }
}
