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
}

[TestFixture]
public class AllowedCoursesContentTests
{
    [Test]
    public void WhenCourseTypeIsApprenticeship_ThenUsesApprenticeshipCopy()
    {
        var content = AllowedCoursesContent.CreateForCourseType(CourseType.Apprenticeship);

        using (new AssertionScope())
        {
            content.PageHeading.Should().Be("Manage apprenticeships this provider is allowed to deliver");
            content.AddButtonText.Should().Be("Add an apprenticeship");
            content.ListHeading.Should().Be("Apprenticeships this provider can deliver");
            content.EmptyListText.Should().Be("There are currently no apprenticeships added to this list.");
        }
    }

    [Test]
    public void WhenCourseTypeIsShortCourse_ThenUsesUnitCopy()
    {
        var content = AllowedCoursesContent.CreateForCourseType(CourseType.ShortCourse);

        using (new AssertionScope())
        {
            content.PageHeading.Should().Be("Manage apprenticeship units this provider is allowed to deliver");
            content.AddButtonText.Should().Be("Add an apprenticeship unit");
            content.ListHeading.Should().Be("Apprenticeship units this provider can deliver");
            content.EmptyListText.Should().Be("There are currently no apprenticeship units added to this list.");
        }
    }
}
