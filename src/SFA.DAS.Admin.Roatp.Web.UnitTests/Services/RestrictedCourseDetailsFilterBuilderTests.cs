using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class RestrictedCourseDetailsFilterBuilderTests
{
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test]
    public void WhenApplyingProviderNameFilter_ThenMatchesNameOrUkprn()
    {
        const int babingtonUkprn = 10019900;
        const int acornUkprn = 10000001;
        var providers = new List<ProviderCourseModel>
        {
            new() { Ukprn = babingtonUkprn, ProviderName = "BABINGTON LTD", LastDateStarts = null },
            new() { Ukprn = acornUkprn, ProviderName = "ACORN SKILLS TRAINING", LastDateStarts = null }
        };

        var byName = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequestModel { SearchTerm = "acorn" });

        var byUkprn = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequestModel { SearchTerm = babingtonUkprn.ToString() });

        using (new AssertionScope())
        {
            byName.Should().ContainSingle(p => p.ProviderName == "ACORN SKILLS TRAINING");
            byUkprn.Should().ContainSingle(p => p.Ukprn == babingtonUkprn);
        }
    }

    [Test]
    public void WhenApplyingProviderNameFilter_AndUkprnIsPartial_ThenDoesNotMatch()
    {
        const int babingtonUkprn = 10019900;
        var providers = new List<ProviderCourseModel>
        {
            new() { Ukprn = babingtonUkprn, ProviderName = "BABINGTON LTD", LastDateStarts = null }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequestModel { SearchTerm = "10019" });

        filtered.Should().BeEmpty();
    }

    [Test]
    public void WhenApplyingDeliveryStatusFilter_ThenMatchesSelectedStatuses()
    {
        var providers = new List<ProviderCourseModel>
        {
            new() { Ukprn = 1, ProviderName = "Open", LastDateStarts = null },
            new() { Ukprn = 2, ProviderName = "Closed", LastDateStarts = DateTime.UtcNow.Date.AddDays(-1) }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequestModel
            {
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            });

        filtered.Should().ContainSingle(p => p.ProviderName == "Closed");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndFiltersAreApplied_ThenShowFilterOptionsIsTrue()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.ShowFilterOptions.Should().BeTrue();
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenSetsLarsCode()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.LarsCode.Should().Be(LarsCode);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenHasTwoFilterSections()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.FilterSections.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenSetsFilterResultsFragment()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.FilterResultsFragment.Should().Be(RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsClearFilterSections()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            filters.ClearFilterSections.Should().HaveCount(2);
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == SearchTermSectionHeading
                && section.Items.Single().DisplayText == "Beacon");
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == DeliveryStatusSectionHeading
                && section.Items.Single().DisplayText == "Last start date added");
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenClearProviderNameLinkKeepsDeliveryStatus()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        var clearProviderLink = filters.ClearFilterSections
            .Single(section => section.Title == SearchTermSectionHeading)
            .Items.Single().ClearLink;

        clearProviderLink.Should().Be(
            $"{RestrictedCourseDetailsUrl}?DeliveryStatus=LastStartDateAdded#{RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment}");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingLastFilter_ThenClearLinkIsBaseUrl()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            SearchTerm = "Beacon"
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(requestModel, LarsCode, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().ClearLink
            .Should().Be($"{RestrictedCourseDetailsUrl}#{RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment}");
    }

    [Test]
    public void WhenApplyingBothFilters_ThenRequiresNameAndStatusMatch()
    {
        var providers = new List<ProviderCourseModel>
        {
            new() { Ukprn = 1, ProviderName = "Beacon Open", LastDateStarts = null },
            new() { Ukprn = 2, ProviderName = "Beacon Closed", LastDateStarts = DateTime.UtcNow.Date.AddDays(-1) },
            new() { Ukprn = 3, ProviderName = "Other Closed", LastDateStarts = DateTime.UtcNow.Date.AddDays(-1) }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequestModel
            {
                SearchTerm = "Beacon",
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            });

        filtered.Should().ContainSingle(p => p.ProviderName == "Beacon Closed");
    }

    [Test]
    public void WhenApplyingNoFilters_ThenReturnsAllProviders()
    {
        var providers = new List<ProviderCourseModel>
        {
            new() { Ukprn = 1, ProviderName = "A", LastDateStarts = null },
            new() { Ukprn = 2, ProviderName = "B", LastDateStarts = DateTime.UtcNow.Date.AddDays(-1) }
        };

        var filtered = RestrictedCourseDetailsFilterBuilder.ApplyFilters(
            providers,
            new GetRestrictedCourseDetailsRequestModel());

        filtered.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermIsNullOrWhitespace_ThenSearchTermFilterNotAdded()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            SearchTerm = "   ",
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(requestModel, LarsCode, urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ClearFilterSections.Should().ContainSingle();
            filters.ClearFilterSections.Should().NotContain(section => section.Title == SearchTermSectionHeading);
            filters.ClearFilterSections.Should().Contain(section => section.Title == DeliveryStatusSectionHeading);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndNoFiltersSelected_ThenHasNoClearSections()
    {
        var urlHelper = CreateUrlHelper();

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(
            new GetRestrictedCourseDetailsRequestModel(),
            LarsCode,
            urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ShowFilterOptions.Should().BeFalse();
            filters.ClearFilterSections.Should().BeEmpty();
            filters.FilterSections.Should().HaveCount(2);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenMarksSelectedDeliveryStatusItems()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(requestModel, LarsCode, urlHelper.Object);

        using (new AssertionScope())
        {
            var deliveryStatusSection = filters.FilterSections
                .OfType<CheckboxListFilterSectionViewModel>()
                .Single();

            deliveryStatusSection.Items.Should().ContainSingle(item => item.IsSelected);
            deliveryStatusSection.Items.Single(item => item.IsSelected).Value
                .Should().Be(nameof(DeliveryStatus.ClosedToNewStarts));
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingOneOfMultipleStatuses_ThenKeepsRemainingStatus()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            DeliveryStatus = [DeliveryStatus.OpenToNewStarts, DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(requestModel, LarsCode, urlHelper.Object);

        var clearOpenLink = filters.ClearFilterSections
            .Single(section => section.Title == DeliveryStatusSectionHeading)
            .Items.Single(item => item.DisplayText == "Open to new starts")
            .ClearLink;

        clearOpenLink.Should().Be(
            $"{RestrictedCourseDetailsUrl}?DeliveryStatus=ClosedToNewStarts#{RestrictedCourseDetailsFilterBuilder.ProviderFilterResultsFragment}");
    }

    private static FiltersViewModel CreateFiltersViewModelWithSelectedFilters()
    {
        var requestModel = new GetRestrictedCourseDetailsRequestModel
        {
            SearchTerm = "Beacon",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
        };

        return RestrictedCourseDetailsFilterBuilder.CreateFiltersViewModel(
            requestModel,
            LarsCode,
            CreateUrlHelper().Object);
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
                    return RestrictedCourseDetailsUrl;
                }

                return null;
            });

        return urlHelper;
    }
}
