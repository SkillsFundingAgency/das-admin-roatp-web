using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class RestrictedCourseDetailsViewModelTests
{
    [Test, MoqAutoData]
    public void WhenMappingFromResponse_AndProvidersExist_ThenMapsCourseDetailsAndSortsProviders(
        GetRestrictedCourseDetailsResponse response)
    {
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Route = "Education and early years";
        response.LearningType = LearningType.Apprenticeship;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 2, ProviderName = "Zebra Training", LastDateStarts = null },
            new ProviderCourseModel { Ukprn = 1, ProviderName = "Alpha Training", LastDateStarts = DateTime.UtcNow.Date.AddDays(-1) }
        ];

        RestrictedCourseDetailsViewModel model = response;

        model.DisplayTitle.Should().Be("Academic professional (Level 7)");
        model.LearningTypeDescription.Should().Be("Apprenticeship");
        model.Sector.Should().Be("Education and early years");
        model.StatusText.Should().Be("Restricted");
        model.ProviderCountDescription.Should().Be("2 providers");
        model.AllowedProviders.Select(p => p.ProviderName).Should().ContainInOrder("Alpha Training", "Zebra Training");
        model.AllowedProviders.First().DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
        model.AllowedProviders.Last().DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
        model.ShowProviderResults.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public void WhenMappingFromResponse_AndNoProviders_ThenSetsEmptyStateFlags(
        GetRestrictedCourseDetailsResponse response)
    {
        response.IsCourseRestricted = true;
        response.Providers = [];

        RestrictedCourseDetailsViewModel model = response;

        model.HasProviders.Should().BeFalse();
        model.HasNoProviders.Should().BeTrue();
        model.ShowProviderResults.Should().BeFalse();
        model.ProviderCountDescription.Should().Be("0 providers");
    }

    [Test, MoqAutoData]
    public void WhenIsCourseRestrictedIsFalse_ThenStatusTextIsNotRestricted(
        GetRestrictedCourseDetailsResponse response)
    {
        response.IsCourseRestricted = false;
        response.Providers = [];

        RestrictedCourseDetailsViewModel model = response;

        model.StatusText.Should().Be("Unrestricted");
    }

    [Test]
    public void WhenFiltersAppliedWithNoMatches_ThenShowsNoFilterResults()
    {
        var model = new RestrictedCourseDetailsViewModel
        {
            LarsCode = "124",
            CourseName = "Course",
            Title = "Course",
            Sector = "Sector",
            HasActiveFilters = true,
            AllowedProviders = []
        };

        model.HasNoFilterResults.Should().BeTrue();
        model.HasNoProviders.Should().BeFalse();
        model.ShowProviderResults.Should().BeTrue();
    }

    [Test]
    public void WhenSuccessBannerMessageIsSet_ThenHasSuccessBannerIsTrue()
    {
        var model = new RestrictedCourseDetailsViewModel
        {
            LarsCode = "124",
            CourseName = "Course",
            Title = "Course",
            Sector = "Sector",
            SuccessBannerMessage = "Last start date added for BP TRAINING"
        };

        model.HasSuccessBanner.Should().BeTrue();
    }

    [Test]
    public void WhenSuccessBannerMessageIsBlank_ThenHasSuccessBannerIsFalse()
    {
        var model = new RestrictedCourseDetailsViewModel
        {
            LarsCode = "124",
            CourseName = "Course",
            Title = "Course",
            Sector = "Sector",
            SuccessBannerMessage = " "
        };

        model.HasSuccessBanner.Should().BeFalse();
    }

    [Test]
    public void BackLinkText_ReturnsExpectedText()
    {
        var model = new RestrictedCourseDetailsViewModel
        {
            LarsCode = "124",
            CourseName = "Course",
            Title = "Course",
            Sector = "Sector"
        };

        model.RestrictedCourseDetailsPageUrl.Should().Be("#");
        model.HasSuccessBanner.Should().BeFalse();
    }
}
