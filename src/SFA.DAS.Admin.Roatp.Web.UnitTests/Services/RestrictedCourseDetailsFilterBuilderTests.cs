using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class RestrictedCourseDetailsFilterBuilderTests
{
    [Test]
    public void WhenApplyingProviderNameFilter_ThenMatchesNameOrUkprn()
    {
        var providers = new List<AllowedProviderViewModel>
        {
            new() { Ukprn = 10019900, ProviderName = "BABINGTON LTD", DeliveryStatus = DeliveryStatus.OpenToNewStarts },
            new() { Ukprn = 10000001, ProviderName = "ACORN SKILLS TRAINING", DeliveryStatus = DeliveryStatus.OpenToNewStarts }
        };

        var byName = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequest { SearchTerm = "acorn" });

        byName.Should().ContainSingle(p => p.ProviderName == "ACORN SKILLS TRAINING");

        var byUkprn = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequest { SearchTerm = "10019900" });

        byUkprn.Should().ContainSingle(p => p.Ukprn == 10019900);
    }

    [Test]
    public void WhenApplyingDeliveryStatusFilter_ThenMatchesSelectedStatuses()
    {
        var providers = new List<AllowedProviderViewModel>
        {
            new() { Ukprn = 1, ProviderName = "Open", DeliveryStatus = DeliveryStatus.OpenToNewStarts },
            new() { Ukprn = 2, ProviderName = "Closed", DeliveryStatus = DeliveryStatus.ClosedToNewStarts }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequest
            {
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            });

        filtered.Should().ContainSingle(p => p.ProviderName == "Closed");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsSectionsAndClearLinks()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCourseDetailsRequest
        {
            SearchTerm = "Beacon",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(request, "105", urlHelper.Object);

        filters.ShowFilterOptions.Should().BeTrue();
        filters.LarsCode.Should().Be("105");
        filters.FilterSections.Should().HaveCount(2);
        filters.ClearFilterSections.Should().HaveCount(2);
        filters.ClearFilterSections.Should().Contain(section =>
            section.Title == SearchTermSectionHeading
            && section.Items.Single().DisplayText == "Beacon");
        filters.ClearFilterSections.Should().Contain(section =>
            section.Title == DeliveryStatusSectionHeading
            && section.Items.Single().DisplayText == "Last start date added");

        var clearProviderLink = filters.ClearFilterSections
            .Single(section => section.Title == SearchTermSectionHeading)
            .Items.Single().ClearLink;

        clearProviderLink.Should().Be(
            $"/restricted-courses/105?DeliveryStatus=LastStartDateAdded#{RestrictedCourseDetailsFilterBuilder.ProviderResultsFragment}");
        filters.Fragment.Should().Be(RestrictedCourseDetailsFilterBuilder.ProviderResultsFragment);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingLastFilter_ThenClearLinkIsBaseUrl()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCourseDetailsRequest
        {
            SearchTerm = "Beacon"
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(request, "105", urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().ClearLink
            .Should().Be($"/restricted-courses/105#{RestrictedCourseDetailsFilterBuilder.ProviderResultsFragment}");
    }

    [Test]
    public void WhenApplyingBothFilters_ThenRequiresNameAndStatusMatch()
    {
        var providers = new List<AllowedProviderViewModel>
        {
            new() { Ukprn = 1, ProviderName = "Beacon Open", DeliveryStatus = DeliveryStatus.OpenToNewStarts },
            new() { Ukprn = 2, ProviderName = "Beacon Closed", DeliveryStatus = DeliveryStatus.ClosedToNewStarts },
            new() { Ukprn = 3, ProviderName = "Other Closed", DeliveryStatus = DeliveryStatus.ClosedToNewStarts }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequest
            {
                SearchTerm = "Beacon",
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            });

        filtered.Should().ContainSingle(p => p.ProviderName == "Beacon Closed");
    }

    [Test]
    public void WhenApplyingNoFilters_ThenReturnsAllProviders()
    {
        var providers = new List<AllowedProviderViewModel>
        {
            new() { Ukprn = 1, ProviderName = "A", DeliveryStatus = DeliveryStatus.OpenToNewStarts },
            new() { Ukprn = 2, ProviderName = "B", DeliveryStatus = DeliveryStatus.ClosedToNewStarts }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequest());

        filtered.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermIsNullOrWhitespace_ThenSearchTermFilterNotAdded()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCourseDetailsRequest
        {
            SearchTerm = "   ",
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(request, "105", urlHelper.Object);

        filters.ClearFilterSections.Should().ContainSingle();
        filters.ClearFilterSections.Should().NotContain(section => section.Title == SearchTermSectionHeading);
        filters.ClearFilterSections.Should().Contain(section => section.Title == DeliveryStatusSectionHeading);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndNoFiltersSelected_ThenHasNoClearSections()
    {
        var urlHelper = CreateUrlHelper();

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(
            new GetRestrictedCourseDetailsRequest(),
            "105",
            urlHelper.Object);

        filters.ShowFilterOptions.Should().BeFalse();
        filters.ClearFilterSections.Should().BeEmpty();
        filters.FilterSections.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenMarksSelectedDeliveryStatusItems()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(request, "105", urlHelper.Object);

        var deliveryStatusSection = filters.FilterSections
            .OfType<SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents.CheckboxListFilterSectionViewModel>()
            .Single();

        deliveryStatusSection.Items.Should().ContainSingle(item => item.IsSelected);
        deliveryStatusSection.Items.Single(item => item.IsSelected).Value
            .Should().Be(nameof(DeliveryStatus.ClosedToNewStarts));
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingOneOfMultipleStatuses_ThenKeepsRemainingStatus()
    {
        var urlHelper = CreateUrlHelper();
        var request = new GetRestrictedCourseDetailsRequest
        {
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts, DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(request, "105", urlHelper.Object);

        var clearOpenLink = filters.ClearFilterSections
            .Single(section => section.Title == DeliveryStatusSectionHeading)
            .Items.Single(item => item.DisplayText == "Open to new starts")
            .ClearLink;

        clearOpenLink.Should().Be(
            $"/restricted-courses/105?DeliveryStatus=ClosedToNewStarts#{RestrictedCourseDetailsFilterBuilder.ProviderResultsFragment}");
    }

    private static Mock<IUrlHelper> CreateUrlHelper()
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((UrlRouteContext context) =>
            {
                if (context.RouteName == RouteNames.RestrictedCourseDetails)
                {
                    return "/restricted-courses/105";
                }

                return null;
            });

        return urlHelper;
    }
}
