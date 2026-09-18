using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.CourseRestrictions;

[TestFixture]
public class GetRestrictedCourseDetailsRequestModelTests
{
    [Test]
    public void HasSearchTermFilter_WhenSearchTermIsEmpty_ThenIsFalse()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel();

        var result = requestModel.HasSearchTermFilter;

        result.Should().BeFalse();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermIsWhitespace_ThenIsFalse()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel { SearchTerm = "   " };

        var result = requestModel.HasSearchTermFilter;

        result.Should().BeFalse();
    }

    [Test]
    public void HasSearchTermFilter_WhenSearchTermHasValue_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel { SearchTerm = "Beacon" };

        var result = requestModel.HasSearchTermFilter;

        result.Should().BeTrue();
    }

    [Test]
    public void HasDeliveryStatusFilter_WhenNoStatuses_ThenIsFalse()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel();

        var result = requestModel.HasDeliveryStatusFilter;

        result.Should().BeFalse();
    }

    [Test]
    public void HasDeliveryStatusFilter_WhenStatusesSelected_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts]
        };

        var result = requestModel.HasDeliveryStatusFilter;

        result.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenSearchTermPresent_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel { SearchTerm = "Beacon" };

        var result = requestModel.HasFilters;

        result.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenDeliveryStatusPresent_ThenIsTrue()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        var result = requestModel.HasFilters;

        result.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenNoFiltersPresent_ThenIsFalse()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel();

        var result = requestModel.HasFilters;

        result.Should().BeFalse();
    }

    [Test]
    public void PageNumber_WhenNotSet_ThenDefaultsToOne()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel();

        requestModel.PageNumber.Should().Be(1);
    }

    [Test]
    public void ToQueryString_WhenFiltersPresent_ThenIncludesSearchTermAndDeliveryStatus()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            SearchTerm = " Beacon ",
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts, DeliveryStatus.ClosedToNewStarts]
        };

        var result = requestModel.ToQueryString();

        result.Should().Contain((nameof(GetRestrictedCourseDetailsRequestModel.SearchTerm), "Beacon"));
        result.Should().Contain((nameof(GetRestrictedCourseDetailsRequestModel.DeliveryStatus), nameof(DeliveryStatus.OpenToNewStarts)));
        result.Should().Contain((nameof(GetRestrictedCourseDetailsRequestModel.DeliveryStatus), nameof(DeliveryStatus.ClosedToNewStarts)));
        result.Should().NotContain(q => q.Item1 == nameof(GetRestrictedCourseDetailsRequestModel.PageNumber));
    }

    [Test]
    public void ToQueryString_WhenNoFiltersPresent_ThenReturnsEmpty()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel();

        requestModel.ToQueryString().Should().BeEmpty();
    }
}
