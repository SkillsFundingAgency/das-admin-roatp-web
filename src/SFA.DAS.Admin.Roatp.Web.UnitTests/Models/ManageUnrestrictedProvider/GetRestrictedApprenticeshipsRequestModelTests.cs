using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageUnrestrictedProvider;

[TestFixture]
public class GetRestrictedApprenticeshipsRequestModelTests
{
    [Test]
    public void WhenNoFiltersSelected_ThenHasFiltersIsFalse()
    {
        var requestModel = new GetRestrictedApprenticeshipsRequestModel();

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
        var requestModel = new GetRestrictedApprenticeshipsRequestModel
        {
            SearchTerm = "Paint"
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
        var requestModel = new GetRestrictedApprenticeshipsRequestModel
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
        var requestModel = new GetRestrictedApprenticeshipsRequestModel
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
        var requestModel = new GetRestrictedApprenticeshipsRequestModel
        {
            SearchTerm = "Paint",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts]
        };

        using (new AssertionScope())
        {
            requestModel.HasSearchTermFilter.Should().BeTrue();
            requestModel.HasDeliveryStatusFilter.Should().BeTrue();
            requestModel.HasFilters.Should().BeTrue();
        }
    }

    [Test]
    public void PageNumber_WhenNotSet_ThenDefaultsToOne()
    {
        var requestModel = new GetRestrictedApprenticeshipsRequestModel();

        requestModel.PageNumber.Should().Be(1);
    }

    [Test]
    public void ToQueryString_WhenFiltersApplied_ThenIncludesSearchTermAndDeliveryStatus()
    {
        var requestModel = new GetRestrictedApprenticeshipsRequestModel
        {
            SearchTerm = " Paint ",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts],
            PageNumber = 2
        };

        var queryString = requestModel.ToQueryString();

        using (new AssertionScope())
        {
            queryString.Should().Contain((nameof(GetRestrictedApprenticeshipsRequestModel.SearchTerm), "Paint"));
            queryString.Should().Contain((nameof(GetRestrictedApprenticeshipsRequestModel.DeliveryStatus), nameof(DeliveryStatus.LastStartDateAdded)));
            queryString.Should().Contain((nameof(GetRestrictedApprenticeshipsRequestModel.DeliveryStatus), nameof(DeliveryStatus.ClosedToNewStarts)));
            queryString.Should().NotContain(q => q.Item1 == nameof(GetRestrictedApprenticeshipsRequestModel.PageNumber));
        }
    }

    [Test]
    public void ToQueryString_WhenNoFilters_ThenReturnsEmpty()
    {
        var requestModel = new GetRestrictedApprenticeshipsRequestModel();

        requestModel.ToQueryString().Should().BeEmpty();
    }
}
