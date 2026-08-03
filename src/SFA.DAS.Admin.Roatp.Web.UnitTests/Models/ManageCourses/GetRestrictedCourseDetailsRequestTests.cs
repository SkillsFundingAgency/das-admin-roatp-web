using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class GetRestrictedCourseDetailsRequestTests
{
    [Test]
    public void HasSearchTermFilter_WhenSearchTermIsEmpty_ThenIsFalse()
    {
        var request = new GetRestrictedCourseDetailsRequest();

        var result = request.HasSearchTermFilter;

        result.Should().BeFalse();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermIsWhitespace_ThenIsFalse()
    {
        var request = new GetRestrictedCourseDetailsRequest { SearchTerm = "   " };

        var result = request.HasSearchTermFilter;

        result.Should().BeFalse();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermHasValue_ThenIsTrue()
    {
        var request = new GetRestrictedCourseDetailsRequest { SearchTerm = "Beacon" };

        var result = request.HasSearchTermFilter;

        result.Should().BeTrue();
    }

    [Test]
    public void HasDeliveryStatusFilter_WhenNoStatuses_ThenIsFalse()
    {
        var request = new GetRestrictedCourseDetailsRequest();

        var result = request.HasDeliveryStatusFilter;

        result.Should().BeFalse();
    }

    [Test]
    public void HasDeliveryStatusFilter_WhenStatusesSelected_ThenIsTrue()
    {
        var request = new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts]
        };

        var result = request.HasDeliveryStatusFilter;

        result.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenSearchTermPresent_ThenIsTrue()
    {
        var request = new GetRestrictedCourseDetailsRequest { SearchTerm = "Beacon" };

        var result = request.HasFilters;

        result.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenDeliveryStatusPresent_ThenIsTrue()
    {
        var request = new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        var result = request.HasFilters;

        result.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenNoFiltersPresent_ThenIsFalse()
    {
        var request = new GetRestrictedCourseDetailsRequest();

        var result = request.HasFilters;

        result.Should().BeFalse();
    }
}
