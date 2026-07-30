using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class GetRestrictedCourseDetailsRequestTests
{
    [Test]
    public void HasProviderNameFilter_WhenProviderNameIsBlank_ThenIsFalse()
    {
        new GetRestrictedCourseDetailsRequest { ProviderName = null }.HasProviderNameFilter.Should().BeFalse();
        new GetRestrictedCourseDetailsRequest { ProviderName = "   " }.HasProviderNameFilter.Should().BeFalse();
    }

    [Test]
    public void HasProviderNameFilter_WhenProviderNameHasValue_ThenIsTrue()
    {
        new GetRestrictedCourseDetailsRequest { ProviderName = "Beacon" }.HasProviderNameFilter.Should().BeTrue();
    }

    [Test]
    public void HasDeliveryStatusFilter_WhenNoStatuses_ThenIsFalse()
    {
        new GetRestrictedCourseDetailsRequest().HasDeliveryStatusFilter.Should().BeFalse();
    }

    [Test]
    public void HasDeliveryStatusFilter_WhenStatusesSelected_ThenIsTrue()
    {
        new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts]
        }.HasDeliveryStatusFilter.Should().BeTrue();
    }

    [Test]
    public void HasFilters_WhenEitherFilterPresent_ThenIsTrue()
    {
        new GetRestrictedCourseDetailsRequest { ProviderName = "Beacon" }.HasFilters.Should().BeTrue();
        new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        }.HasFilters.Should().BeTrue();
        new GetRestrictedCourseDetailsRequest().HasFilters.Should().BeFalse();
    }
}
