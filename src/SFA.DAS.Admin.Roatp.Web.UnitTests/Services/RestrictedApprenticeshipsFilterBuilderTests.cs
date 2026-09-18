using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Filters.FilterComponents;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using static SFA.DAS.Admin.Roatp.Web.Services.FilterService;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

[TestFixture]
public class RestrictedApprenticeshipsFilterBuilderTests
{
    private const int Ukprn = 10019900;
    private const string RestrictedApprenticeshipsUrl = "/providers/10019900/restricted-courses";

    [Test]
    public void WhenApplyingCourseNameFilter_ThenMatchesDisplayTitleOrLarsCode()
    {
        var courses = CreateCourses();

        var byName = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(courses,
            new GetRestrictedApprenticeshipsModel { SearchTerm = "cleaning" }).ToList();

        var byLarsCode = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(courses,
            new GetRestrictedApprenticeshipsModel { SearchTerm = "124" }).ToList();

        using (new AssertionScope())
        {
            byName.Should().ContainSingle(c => c.LarsCode == "163");
            byLarsCode.Should().ContainSingle(c => c.LarsCode == "124");
        }
    }

    [Test]
    public void WhenApplyingCourseNameFilter_AndLarsCodeIsPartial_ThenDoesNotMatch()
    {
        var courses = CreateCourses();

        var filtered = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(courses,
            new GetRestrictedApprenticeshipsModel { SearchTerm = "12" }).ToList();

        filtered.Should().BeEmpty();
    }

    [Test]
    public void WhenApplyingDeliveryStatusFilter_ThenMatchesSelectedStatuses()
    {
        var courses = CreateCourses();

        var filtered = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(courses,
            new GetRestrictedApprenticeshipsModel
            {
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts]
            }).ToList();

        using (new AssertionScope())
        {
            filtered.Should().OnlyContain(c => c.DeliveryStatus == DeliveryStatus.ClosedToNewStarts);
            filtered.Should().HaveCount(2);
        }
    }

    [Test]
    public void WhenApplyingBothFilters_ThenRequiresTitleAndStatusMatch()
    {
        var courses = CreateCourses();

        var filtered = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(courses,
            new GetRestrictedApprenticeshipsModel
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

        var filtered = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(courses,
            new GetRestrictedApprenticeshipsModel()).ToList();

        filtered.Should().HaveCount(courses.Count);
    }

    [Test]
    public void WhenApplyingDuplicateDeliveryStatusValues_ThenMatchesDistinctStatuses()
    {
        var courses = CreateCourses();

        var filtered = RestrictedApprenticeshipsFilterBuilder.ApplyFilters(
            courses,
            new GetRestrictedApprenticeshipsModel
            {
                DeliveryStatus = [DeliveryStatus.ClosedToNewStarts, DeliveryStatus.ClosedToNewStarts]
            }).ToList();

        using (new AssertionScope())
        {
            filtered.Should().OnlyContain(c => c.DeliveryStatus == DeliveryStatus.ClosedToNewStarts);
            filtered.Should().HaveCount(2);
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermIsNull_ThenSearchTermFilterIsNotSelected()
    {
        var urlHelper = CreateUrlHelper();
        var model = new GetRestrictedApprenticeshipsModel
        {
            SearchTerm = null!
        };

        var filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, Ukprn, urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ShowFilterOptions.Should().BeFalse();
            filters.FilterSections[0].Should().BeOfType<TextBoxFilterSectionViewModel>()
                .Which.InputValue.Should().BeEmpty();
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_ThenBuildsCourseNameAndDeliveryStatusSections()
    {
        var urlHelper = CreateUrlHelper();
        var model = new GetRestrictedApprenticeshipsModel
        {
            SearchTerm = "Paint",
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded]
        };

        var filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, Ukprn, urlHelper.Object);

        using (new AssertionScope())
        {
            filters.ShowFilterOptions.Should().BeTrue();
            filters.Ukprn.Should().Be(Ukprn);
            filters.LarsCode.Should().BeNull();
            filters.Route.Should().Be(RouteNames.ProviderRestrictedCourses);
            filters.FilterResultsFragment.Should().Be(
                RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment);
            filters.FilterSections.Should().HaveCount(2);

            var searchSection = filters.FilterSections[0].Should().BeOfType<TextBoxFilterSectionViewModel>().Subject;
            searchSection.Heading.Should().Be(CourseNameSectionHeading);
            searchSection.SubHeading.Should().Be(CourseNameSectionSubHeading);
            searchSection.InputValue.Should().Be("Paint");

            var deliveryStatusSection = filters.FilterSections[1].Should().BeOfType<CheckboxListFilterSectionViewModel>().Subject;
            deliveryStatusSection.Heading.Should().Be(DeliveryStatusSectionHeading);
            deliveryStatusSection.Items.Select(item => item.Value).Should().Equal(
                nameof(DeliveryStatus.LastStartDateAdded),
                nameof(DeliveryStatus.ClosedToNewStarts));
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.LastStartDateAdded)).IsSelected.Should().BeTrue();
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.ClosedToNewStarts)).IsSelected.Should().BeFalse();
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.LastStartDateAdded))
                .DisplayDescription.Should().Be("Course will be restricted after this date");
            deliveryStatusSection.Items.Single(i => i.Value == nameof(DeliveryStatus.ClosedToNewStarts))
                .DisplayDescription.Should().Be("This will be removed once all learners have completed the course");

            filters.ClearFilterSections.Should().HaveCount(2);
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == CourseNameSectionHeading
                && section.Items.Single().DisplayText == "Paint");
            filters.ClearFilterSections.Should().Contain(section =>
                section.Title == DeliveryStatusSectionHeading
                && section.Items.Single().DisplayText == "Last start date added");

            var clearCourseNameLink = filters.ClearFilterSections
                .Single(section => section.Title == CourseNameSectionHeading)
                .Items.Single().ClearLink;

            clearCourseNameLink.Should().Be(
                $"{RestrictedApprenticeshipsUrl}?DeliveryStatus=LastStartDateAdded#{RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment}");
        }
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndSearchTermHasSurroundingWhitespace_ThenTrimsSearchTermForClearLink()
    {
        var urlHelper = CreateUrlHelper();
        var model = new GetRestrictedApprenticeshipsModel
        {
            SearchTerm = "  Paint  "
        };

        var filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, Ukprn, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().DisplayText.Should().Be("Paint");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndNoFiltersSelected_ThenHasNoClearSections()
    {
        var urlHelper = CreateUrlHelper();

        var filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(
            new GetRestrictedApprenticeshipsModel(),
            Ukprn,
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
        var model = new GetRestrictedApprenticeshipsModel
        {
            SearchTerm = "Paint"
        };

        var filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, Ukprn, urlHelper.Object);

        filters.ClearFilterSections.Single().Items.Single().ClearLink.Should().Be(
            $"{RestrictedApprenticeshipsUrl}#{RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment}");
    }

    [Test]
    public void WhenCreatingFiltersViewModel_AndClearingOneOfMultipleStatuses_ThenKeepsRemainingStatus()
    {
        var urlHelper = CreateUrlHelper();
        var model = new GetRestrictedApprenticeshipsModel
        {
            DeliveryStatus = [DeliveryStatus.LastStartDateAdded, DeliveryStatus.ClosedToNewStarts]
        };

        var filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(model, Ukprn, urlHelper.Object);

        var clearLastStartDateLink = filters.ClearFilterSections
            .Single(section => section.Title == DeliveryStatusSectionHeading)
            .Items.Single(item => item.DisplayText == "Last start date added")
            .ClearLink;

        clearLastStartDateLink.Should().Be(
            $"{RestrictedApprenticeshipsUrl}?DeliveryStatus=ClosedToNewStarts#{RestrictedApprenticeshipsFilterBuilder.RestrictedApprenticeshipFilterResultsFragment}");
    }

    private static List<RestrictedApprenticeshipItemViewModel> CreateCourses() =>
    [
        new()
        {
            LarsCode = "124",
            Title = "Chartered manager course",
            Level = 6,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
            IsClosedToNewStarts = false,
            DeliveryStatus = DeliveryStatus.LastStartDateAdded
        },
        new()
        {
            LarsCode = "163",
            Title = "Cleaning hygiene operative",
            Level = 2,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
            IsClosedToNewStarts = true,
            DeliveryStatus = DeliveryStatus.ClosedToNewStarts
        },
        new()
        {
            LarsCode = "999",
            Title = "Paint unit",
            Level = 3,
            LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
            IsClosedToNewStarts = true,
            DeliveryStatus = DeliveryStatus.ClosedToNewStarts
        }
    ];

    private static Mock<IUrlHelper> CreateUrlHelper()
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((UrlRouteContext context) =>
            {
                context.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
                return RestrictedApprenticeshipsUrl;
            });
        return urlHelper;
    }
}
