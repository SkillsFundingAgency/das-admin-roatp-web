using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class AllowedCoursesFilterBuilderTests
{
    private const int Ukprn = 10019900;
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";

    [Test]
    public void WhenApplyingCourseNameFilterByName_ThenMatchesDisplayTitle()
    {
        var courses = CreateCourses();

        var byName = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel { SearchTerm = "Cleaning" }).ToList();

        byName.Should().ContainSingle(c => c.Title == "Cleaning hygiene operative");
    }

    [Test]
    public void WhenApplyingCourseNameFilterByLarsCode_ThenMatchesLarsCode()
    {
        var courses = CreateCourses();

        var byLarsCode = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel { SearchTerm = "124" }).ToList();

        byLarsCode.Should().ContainSingle(c => c.LarsCode == "124");
    }

    [Test]
    public void WhenApplyingCourseNameFilter_AndLarsCodeIsPartial_ThenDoesNotMatch()
    {
        var courses = CreateCourses();

        var filtered = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel { SearchTerm = "12" }).ToList();

        filtered.Should().BeEmpty();
    }

    [Test]
    public void WhenApplyingDeliveryStatusFilter_ThenMatchesSelectedStatuses()
    {
        var courses = CreateCourses();

        var filtered = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel
            {
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            }).ToList();

        using (new AssertionScope())
        {
            filtered.Select(c => c.LarsCode).Should().BeEquivalentTo("163", "999");
            filtered.Should().HaveCount(2);
        }
    }

    [Test]
    public void WhenApplyingOpenToNewStartsFilter_ThenMatchesCoursesWithNoLastStartDate()
    {
        var courses = CreateCourses();

        var filtered = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel
            {
                DeliveryStatus = [DeliveryStatus.OpenToNewStarts]
            }).ToList();

        filtered.Should().ContainSingle(c => c.LarsCode == "150");
    }

    [Test]
    public void WhenApplyingBothFilters_ThenRequiresTitleAndStatusMatch()
    {
        var courses = CreateCourses();

        var filtered = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel
            {
                SearchTerm = "course",
                DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
            }).ToList();

        filtered.Should().ContainSingle(c => c.LarsCode == "124");
    }

    [Test]
    public void WhenApplyingNoFilters_ThenReturnsAllCourses()
    {
        var courses = CreateCourses();

        var filtered = AllowedCoursesFilterBuilder.ApplyFilters(courses,
            new GetProviderAllowedCoursesRequestModel()).ToList();

        filtered.Should().HaveCount(courses.Count);
    }

    [Test]
    public void WhenApplyingDuplicateDeliveryStatusValues_ThenMatchesDistinctStatuses()
    {
        var courses = CreateCourses();

        var filtered = AllowedCoursesFilterBuilder.ApplyFilters(
            courses,
            new GetProviderAllowedCoursesRequestModel
            {
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts, DeliveryStatus.ClosedToNewStarts]
            }).ToList();

        filtered.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermIsNull_ThenSearchTermFilterIsNotSelected()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = null!
        };

        var filters = AllowedCoursesFilterBuilder.CreateFiltersViewModel(
            requestModel,
            Ukprn,
            RouteLearningType.Apprenticeship,
            urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ShowFilterOptions.Should().BeFalse();
            filters.FilterSections[0].Should().BeOfType<TextBoxFilterSectionViewModel>()
                .Which.InputValue.Should().BeEmpty();
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndFiltersAreApplied_ThenShowFilterOptionsIsTrue()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.ShowFilterOptions.Should().BeTrue();
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenSetsUkprnAndRouteLearningType()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            filters.Ukprn.Should().Be(Ukprn);
            filters.RouteLearningType.Should().Be(RouteLearningType.Apprenticeship);
            filters.LarsCode.Should().BeNull();
            filters.Route.Should().Be(RouteNames.ProviderAllowedCourses);
            filters.FilterResultsFragment.Should().Be(AllowedCoursesFilterBuilder.AllowedCourseFilterResultsFragment);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenHasTwoFilterSections()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        filters.FilterSections.Should().HaveCount(2);
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsCourseNameSection()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var searchSection = filters.FilterSections[0].Should().BeOfType<TextBoxFilterSectionViewModel>().Subject;
            searchSection.Heading.Should().Be(CourseNameSectionHeading);
            searchSection.SubHeading.Should().Be(CourseNameSectionSubHeading);
            searchSection.InputValue.Should().Be("Beacon");
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsDeliveryStatusSection()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var deliveryStatusSection = filters.FilterSections[1]
                .Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            deliveryStatusSection.Heading.Should().Be(DeliveryStatusSectionHeading);
            deliveryStatusSection.Items.Select(item => item.Value).Should().Equal(
                nameof(DeliveryStatus.OpenToNewStarts),
                nameof(DeliveryStatus.LastStartDateAdded),
                nameof(DeliveryStatus.ClosedToNewStarts));
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenMarksSelectedDeliveryStatus()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var deliveryStatusSection = filters.FilterSections[1]
                .Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.OpenToNewStarts))
                .IsSelected.Should().BeFalse();
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.LastStartDateAdded))
                .IsSelected.Should().BeTrue();
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.ClosedToNewStarts))
                .IsSelected.Should().BeFalse();
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenSetsDeliveryStatusDescriptions()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            var deliveryStatusSection = filters.FilterSections[1]
                .Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.OpenToNewStarts))
                .DisplayDescription.Should().Be("Training providers offer this course on Find apprenticeship training");
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.LastStartDateAdded))
                .DisplayDescription.Should().Be("Training providers will not be able to take on new learners after this date");
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.ClosedToNewStarts))
                .DisplayDescription.Should().Be(ClosedToNewStartsDescription);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsClearFilterSections()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        using (new AssertionScope())
        {
            filters.ClearFilterSections.Should().HaveCount(2);
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == CourseNameSectionHeading
                && section.Items.Single().DisplayText == "Beacon");
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == DeliveryStatusSectionHeading
                && section.Items.Single().DisplayText == "Last start date added");
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenClearCourseNameLinkKeepsDeliveryStatus()
    {
        var filters = CreateFiltersViewModelWithSelectedFilters();

        var clearCourseNameLink = filters.ClearFilterSections
            .Single(section => section.Title == CourseNameSectionHeading)
            .Items.Single().ClearLink;

        clearCourseNameLink.Should().Be(
            $"{AllowedCoursesUrl}?DeliveryStatus=LastStartDateAdded#{AllowedCoursesFilterBuilder.AllowedCourseFilterResultsFragment}");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermHasSurroundingWhitespace_ThenTrimsSearchTermForClearLink()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = "  Beacon  "
        };

        var filters = AllowedCoursesFilterBuilder.CreateFiltersViewModel(
            requestModel,
            Ukprn,
            RouteLearningType.Apprenticeship,
            urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Beacon");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndNoFiltersSelected_ThenHasNoClearSections()
    {
        var urlHelper = CreateUrlHelper();

        var filters = AllowedCoursesFilterBuilder.CreateFiltersViewModel(
            new GetProviderAllowedCoursesRequestModel(),
            Ukprn,
            RouteLearningType.Apprenticeship,
            urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ShowFilterOptions.Should().BeFalse();
            filters.ClearFilterSections.Should().BeEmpty();
            filters.FilterSections.Should().HaveCount(2);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingLastFilter_ThenClearLinkIsBaseUrl()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = "Beacon"
        };

        var filters = AllowedCoursesFilterBuilder.CreateFiltersViewModel(
            requestModel,
            Ukprn,
            RouteLearningType.Apprenticeship,
            urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().ClearLink.Should().Be(
            $"{AllowedCoursesUrl}#{AllowedCoursesFilterBuilder.AllowedCourseFilterResultsFragment}");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingOneOfMultipleStatuses_ThenKeepsRemainingStatus()
    {
        var urlHelper = CreateUrlHelper();
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts]
        };

        var filters = AllowedCoursesFilterBuilder.CreateFiltersViewModel(
            requestModel,
            Ukprn,
            RouteLearningType.Apprenticeship,
            urlHelper.Object);

        var clearLastStartDateLink = filters.ClearFilterSections
            .Single(section => section.Title == DeliveryStatusSectionHeading)
            .Items.Single(item => item.DisplayText == "Last start date added")
            .ClearLink;

        clearLastStartDateLink.Should().Be(
            $"{AllowedCoursesUrl}?DeliveryStatus=ClosedToNewStarts#{AllowedCoursesFilterBuilder.AllowedCourseFilterResultsFragment}");
    }

    private static FiltersViewModel CreateFiltersViewModelWithSelectedFilters()
    {
        var requestModel = new GetProviderAllowedCoursesRequestModel
        {
            SearchTerm = "Beacon",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
        };

        return AllowedCoursesFilterBuilder.CreateFiltersViewModel(
            requestModel,
            Ukprn,
            RouteLearningType.Apprenticeship,
            CreateUrlHelper().Object);
    }

    private static List<ProviderAllowedCourseModel> CreateCourses() =>
    [
        new()
        {
            LarsCode = "124",
            Title = "Chartered manager course",
            Level = 6,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
            IsClosedToNewStarts = false
        },
        new()
        {
            LarsCode = "150",
            Title = "Open course",
            Level = 2,
            LastDateStarts = null,
            IsClosedToNewStarts = false
        },
        new()
        {
            LarsCode = "163",
            Title = "Cleaning hygiene operative",
            Level = 2,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
            IsClosedToNewStarts = true
        },
        new()
        {
            LarsCode = "999",
            Title = "Paint unit",
            Level = 3,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
            IsClosedToNewStarts = true
        }
    ];

    private static Mock<IUrlHelper> CreateUrlHelper()
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((UrlRouteContext context) =>
            {
                context.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
                return AllowedCoursesUrl;
            });
        return urlHelper;
    }
}
