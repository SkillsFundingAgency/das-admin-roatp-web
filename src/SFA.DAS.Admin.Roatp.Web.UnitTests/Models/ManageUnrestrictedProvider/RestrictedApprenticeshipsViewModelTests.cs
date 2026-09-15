using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageUnrestrictedProvider;

[TestFixture]
public class RestrictedApprenticeshipsViewModelTests
{
    [Test]
    public void WhenMappingFromResponse_AndRestrictedCoursesExist_ThenExcludesOpenCoursesAndOrdersByName()
    {
        var response = new GetRestrictedApprenticeshipsResponse
        {
            Courses =
            [
                new RestrictedApprenticeshipModel
                {
                    LarsCode = "200",
                    Title = "Zebra course",
                    Level = 3,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                    IsClosedToNewStarts = false
                },
                new RestrictedApprenticeshipModel
                {
                    LarsCode = "100",
                    Title = "Open course",
                    Level = 2,
                    LastDateStarts = null,
                    IsClosedToNewStarts = false
                },
                new RestrictedApprenticeshipModel
                {
                    LarsCode = "105",
                    Title = "Alpha course",
                    Level = 6,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                    IsClosedToNewStarts = false
                }
            ]
        };

        RestrictedApprenticeshipsViewModel model = response;

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeTrue();
            model.HasNoCourses.Should().BeFalse();
            model.TotalCount.Should().Be(2);
            model.TotalCountDescription.Should().Be("2 courses");
            model.Courses.Select(course => course.LarsCode).Should().Equal("105", "200");
            model.Courses.Should().NotContain(course => course.DeliveryStatus == DeliveryStatus.OpenToNewStarts);
        }
    }

    [Test]
    public void WhenMappingFromResponse_AndNoRestrictedCourses_ThenHasNoCourses()
    {
        var response = new GetRestrictedApprenticeshipsResponse();

        RestrictedApprenticeshipsViewModel model = response;

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeFalse();
            model.HasNoCourses.Should().BeTrue();
            model.TotalCount.Should().Be(0);
            model.TotalCountDescription.Should().Be("0 courses");
            model.Courses.Should().BeEmpty();
        }
    }

    [Test]
    public void WhenMappingFromResponse_AndOnlyOpenCoursesExist_ThenHasNoCourses()
    {
        var response = new GetRestrictedApprenticeshipsResponse
        {
            Courses =
            [
                new RestrictedApprenticeshipModel
                {
                    LarsCode = "100",
                    Title = "Open course",
                    Level = 2,
                    LastDateStarts = null,
                    IsClosedToNewStarts = false
                }
            ]
        };

        RestrictedApprenticeshipsViewModel model = response;

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeFalse();
            model.HasNoCourses.Should().BeTrue();
            model.Courses.Should().BeEmpty();
        }
    }

    [Test]
    public void WhenMappingFromNullResponse_ThenHasNoCourses()
    {
        GetRestrictedApprenticeshipsResponse? response = null;

        RestrictedApprenticeshipsViewModel model = response!;

        using (new AssertionScope())
        {
            model.HasCourses.Should().BeFalse();
            model.HasNoCourses.Should().BeTrue();
            model.Courses.Should().BeEmpty();
        }
    }

    [Test]
    public void WhenMappingFromResponse_AndOneRestrictedCourseExists_ThenTotalCountDescriptionIsSingular()
    {
        var response = new GetRestrictedApprenticeshipsResponse
        {
            Courses =
            [
                new RestrictedApprenticeshipModel
                {
                    LarsCode = "105",
                    Title = "Chartered manager",
                    Level = 6,
                    LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                    IsClosedToNewStarts = false
                }
            ]
        };

        RestrictedApprenticeshipsViewModel model = response;

        model.TotalCountDescription.Should().Be("1 course");
    }
}
