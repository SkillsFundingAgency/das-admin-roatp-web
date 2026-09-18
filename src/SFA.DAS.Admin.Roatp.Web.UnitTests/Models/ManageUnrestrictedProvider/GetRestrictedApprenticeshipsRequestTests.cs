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
        var request = new GetRestrictedApprenticeshipsModel();

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
        var request = new GetRestrictedApprenticeshipsModel
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
        var request = new GetRestrictedApprenticeshipsModel
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
        var request = new GetRestrictedApprenticeshipsModel
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
        var request = new GetRestrictedApprenticeshipsModel
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

    [Test]
    public void PageNumber_WhenNotSet_ThenDefaultsToOne()
    {
        var request = new GetRestrictedApprenticeshipsModel();

        request.PageNumber.Should().Be(1);
    }

    [Test]
    public void ToQueryString_WhenFiltersApplied_ThenIncludesSearchTermAndDeliveryStatus()
    {
        var request = new GetRestrictedApprenticeshipsModel
        {
            SearchTerm = " Paint ",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts],
            PageNumber = 2
        };

        var queryString = request.ToQueryString();

        using (new AssertionScope())
        {
            queryString.Should().Contain((nameof(GetRestrictedApprenticeshipsModel.SearchTerm), "Paint"));
            queryString.Should().Contain((nameof(GetRestrictedApprenticeshipsModel.DeliveryStatus), nameof(DeliveryStatus.LastStartDateAdded)));
            queryString.Should().Contain((nameof(GetRestrictedApprenticeshipsModel.DeliveryStatus), nameof(DeliveryStatus.ClosedToNewStarts)));
            queryString.Should().NotContain(q => q.Item1 == nameof(GetRestrictedApprenticeshipsModel.PageNumber));
        }
    }

    [Test]
    public void ToQueryString_WhenNoFilters_ThenReturnsEmpty()
    {
        var model = new GetRestrictedApprenticeshipsModel();

        model.ToQueryString().Should().BeEmpty();
    }
}
