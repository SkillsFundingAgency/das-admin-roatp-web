using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageRestrictedProvider;

[TestFixture]
public class AllowedCoursesPageContentModelTests
{
    [Test]
    public void WhenCourseTypeIsApprenticeship_ThenUsesApprenticeshipCopy()
    {
        var content = AllowedCoursesPageContentModel.CreateForCourseType(CourseType.Apprenticeship);

        using (new AssertionScope())
        {
            content.PageHeading.Should().Be("Manage apprenticeships this provider is allowed to deliver");
            content.IntroText.Should().Be("This provider's account permissions mean that they can only offer courses we have added to their account.");
            content.AddHeading.Should().Be("Add an apprenticeship");
            content.AddDescription.Should().Be("Adding a new apprenticeship will allow this provider to add it to their account in Course management.");
            AllowedCoursesPageContentModel.AddSecondaryDescription.Should().Be("If they add it in Course management, they will be able to offer it to learners on Find apprenticeship training.");
            content.AddButtonText.Should().Be("Add an apprenticeship");
            content.ListHeading.Should().Be("Apprenticeships this provider can deliver");
            AllowedCoursesPageContentModel.ListIntro.Should().Be("This is a list of what courses this provider can add to their account in Course management and offer in Find apprenticeship training.");
            content.EmptyListText.Should().Be("There are currently no apprenticeships added to this list.");
        }
    }

    [Test]
    public void WhenCourseTypeIsShortCourse_ThenUsesUnitCopy()
    {
        var content = AllowedCoursesPageContentModel.CreateForCourseType(CourseType.ShortCourse);

        using (new AssertionScope())
        {
            content.PageHeading.Should().Be("Manage apprenticeship units this provider is allowed to deliver");
            content.IntroText.Should().Be("This page shows the apprenticeship units the provider is allowed to deliver. You can add or remove apprenticeships units.");
            content.AddHeading.Should().Be("Add an apprenticeship unit");
            content.AddDescription.Should().Be("Adding a new apprenticeship unit will allow this provider to add it to their account in Course management.");
            AllowedCoursesPageContentModel.AddSecondaryDescription.Should().Be("If they add it in Course management, they will be able to offer it to learners on Find apprenticeship training.");
            content.AddButtonText.Should().Be("Add an apprenticeship unit");
            content.ListHeading.Should().Be("Apprenticeship units this provider can deliver");
            AllowedCoursesPageContentModel.ListIntro.Should().Be("This is a list of what courses this provider can add to their account in Course management and offer in Find apprenticeship training.");
            content.EmptyListText.Should().Be("There are currently no apprenticeship units added to this list.");
        }
    }
}
