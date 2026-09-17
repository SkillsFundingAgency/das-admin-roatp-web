using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageUnrestrictedProvider;

[TestFixture]
public class GetRestrictedApprenticeshipsRequestTests
{
    [Test]
    public void WhenNoFiltersSelected_ThenHasFiltersIsFalse()
    {
        var request = new GetRestrictedApprenticeshipsRequest();

        using (new AssertionScope())
        {
            request.HasSearchTermFilter.Should().BeFalse();
            request.HasDeliveryStatusFilter.Should().BeFalse();
            request.HasFilters.Should().BeFalse();
        }
    }

    [Test]
    public void WhenSearchTermProvided_ThenHasSearchTermFilterIsTrue()
    {
        var request = new GetRestrictedApprenticeshipsRequest
        {
            SearchTerm = "Paint"
        };

        using (new AssertionScope())
        {
            request.HasSearchTermFilter.Should().BeTrue();
            request.HasFilters.Should().BeTrue();
        }
    }

    [Test]
    public void WhenSearchTermIsWhitespace_ThenHasSearchTermFilterIsFalse()
    {
        var request = new GetRestrictedApprenticeshipsRequest
        {
            SearchTerm = "   "
        };

        using (new AssertionScope())
        {
            request.HasSearchTermFilter.Should().BeFalse();
            request.HasFilters.Should().BeFalse();
        }
    }

    [Test]
    public void WhenDeliveryStatusSelected_ThenHasDeliveryStatusFilterIsTrue()
    {
        var request = new GetRestrictedApprenticeshipsRequest
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        using (new AssertionScope())
        {
            request.HasDeliveryStatusFilter.Should().BeTrue();
            request.HasFilters.Should().BeTrue();
        }
    }

    [Test]
    public void WhenSearchTermAndDeliveryStatusSelected_ThenHasFiltersIsTrue()
    {
        var request = new GetRestrictedApprenticeshipsRequest
        {
            SearchTerm = "Paint",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts]
        };

        using (new AssertionScope())
        {
            request.HasSearchTermFilter.Should().BeTrue();
            request.HasDeliveryStatusFilter.Should().BeTrue();
            request.HasFilters.Should().BeTrue();
        }
    }
}
