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

    [Test]
    public void WhenPageNumberIsNotSet_ThenDefaultsToOne()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel();

        requestModel.PageNumber.Should().Be(1);
    }

    [Test]
    public void WhenFiltersApplied_ThenToQueryStringIncludesSearchTermAndDeliveryStatus()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = " Beacon ",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts],
            PageNumber = 2
        };

        var queryString = requestModel.ToQueryString();

        using (new AssertionScope())
        {
            queryString.Should().Contain((nameof(GetProviderAllowedCoursesRequestModel.SearchTerm), "Beacon"));
            queryString.Should().Contain((nameof(GetProviderAllowedCoursesRequestModel.DeliveryStatus), nameof(DeliveryStatus.LastStartDateAdded)));
            queryString.Should().Contain((nameof(GetProviderAllowedCoursesRequestModel.DeliveryStatus), nameof(DeliveryStatus.ClosedToNewStarts)));
            queryString.Should().NotContain(q => q.Item1 == nameof(GetProviderAllowedCoursesRequestModel.PageNumber));
        }
    }

    [Test]
    public void WhenNoFilters_ThenToQueryStringReturnsEmpty()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel();

        requestModel.ToQueryString().Should().BeEmpty();
    }
}
