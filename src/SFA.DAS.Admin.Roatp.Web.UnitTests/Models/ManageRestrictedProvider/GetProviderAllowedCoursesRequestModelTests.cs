using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageRestrictedProvider;

[TestFixture]
public class GetProviderAllowedCoursesRequestModelTests
{
    [Test]
    public void WhenNoFiltersSelected_ThenHasFiltersIsFalse()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel();

        using (new AssertionScope())
        {
            requestModel.HasSearchTermFilter.Should().BeFalse();
            requestModel.HasDeliveryStatusFilter.Should().BeFalse();
            requestModel.HasFilters.Should().BeFalse();
        }
    }

    [Test]
    public void WhenSearchTermProvided_ThenHasSearchTermFilterIsTrue()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = "Beacon"
        };

        using (new AssertionScope())
        {
            requestModel.HasSearchTermFilter.Should().BeTrue();
            requestModel.HasFilters.Should().BeTrue();
        }
    }

    [Test]
    public void WhenSearchTermIsWhitespace_ThenHasSearchTermFilterIsFalse()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = "   "
        };

        using (new AssertionScope())
        {
            requestModel.HasSearchTermFilter.Should().BeFalse();
            requestModel.HasFilters.Should().BeFalse();
        }
    }

    [Test]
    public void WhenDeliveryStatusSelected_ThenHasDeliveryStatusFilterIsTrue()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        using (new AssertionScope())
        {
            requestModel.HasDeliveryStatusFilter.Should().BeTrue();
            requestModel.HasFilters.Should().BeTrue();
        }
    }

    [Test]
    public void WhenSearchTermAndDeliveryStatusSelected_ThenHasFiltersIsTrue()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = "Beacon",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts]
        };

        using (new AssertionScope())
        {
            requestModel.HasSearchTermFilter.Should().BeTrue();
            requestModel.HasDeliveryStatusFilter.Should().BeTrue();
            requestModel.HasFilters.Should().BeTrue();
        }
    }
}
