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

    [Test]
    public void PageNumber_WhenNotSet_ThenDefaultsToOne()
    {
        var request = new GetRestrictedCourseDetailsRequest();

        request.PageNumber.Should().Be(1);
    }

    [Test]
    public void ToQueryString_WhenFiltersPresent_ThenIncludesSearchTermAndDeliveryStatus()
    {
        var request = new GetRestrictedCourseDetailsRequest
        {
            SearchTerm = " Beacon ",
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts, DeliveryStatus.ClosedToNewStarts]
        };

        var result = request.ToQueryString();

        result.Should().Contain((nameof(GetRestrictedCourseDetailsRequest.SearchTerm), "Beacon"));
        result.Should().Contain((nameof(GetRestrictedCourseDetailsRequest.DeliveryStatus), nameof(DeliveryStatus.OpenToNewStarts)));
        result.Should().Contain((nameof(GetRestrictedCourseDetailsRequest.DeliveryStatus), nameof(DeliveryStatus.ClosedToNewStarts)));
        result.Should().NotContain(q => q.Item1 == nameof(GetRestrictedCourseDetailsRequest.PageNumber));
    }

    [Test]
    public void ToQueryString_WhenNoFiltersPresent_ThenReturnsEmpty()
    {
        var request = new GetRestrictedCourseDetailsRequest();

        request.ToQueryString().Should().BeEmpty();
    }
}
