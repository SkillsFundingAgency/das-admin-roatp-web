using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.RestrictedCourses;

[TestFixture]
public class RestrictedCourseDetailsViewModelTests
{
    [Test]
    public void WhenMappingFromResponse_AndProvidersExist_ThenMapsCourseDetailsAndSortsProviders()
    {
        var response = new GetRestrictedCourseDetailsResponse
        {
            LarsCode = "105",
            IfateReferenceNumber = "ST0001",
            CourseName = "Academic professional",
            Level = 7,
            Route = "Education and early years",
            LearningType = LearningType.Apprenticeship,
            IsCourseRestricted = true,
            Providers =
            [
                new ProviderCourseModel { Ukprn = 2, ProviderName = "Zebra Training", DateLastStarts = null },
                new ProviderCourseModel { Ukprn = 1, ProviderName = "Alpha Training", DateLastStarts = DateTime.UtcNow.Date.AddDays(-1) }
            ]
        };

        RestrictedCourseDetailsViewModel model = response;

        model.DisplayTitle.Should().Be("Academic professional (Level 7)");
        model.LearningTypeDescription.Should().Be("Apprenticeship");
        model.Sector.Should().Be("Education and early years");
        model.StatusText.Should().Be("Restricted");
        model.ProviderCountDescription.Should().Be("2 providers");
        model.Providers.Select(p => p.ProviderName).Should().ContainInOrder("Alpha Training", "Zebra Training");
        model.Providers.First().DeliveryStatus.Should().Be(DeliveryStatus.ClosedToNewStarts);
        model.Providers.Last().DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
    }

    [Test]
    public void WhenMappingFromResponse_AndNoProviders_ThenSetsEmptyStateFlags()
    {
        var response = new GetRestrictedCourseDetailsResponse
        {
            LarsCode = "105",
            IfateReferenceNumber = "ST0001",
            CourseName = "Academic professional",
            Route = "Education and early years",
            LearningType = LearningType.Apprenticeship,
            IsCourseRestricted = true,
            Providers = []
        };

        RestrictedCourseDetailsViewModel model = response;

        model.HasProviders.Should().BeFalse();
        model.HasNoProviders.Should().BeTrue();
        model.ProviderCountDescription.Should().Be("0 providers");
    }
}

[TestFixture]
public class AllowedProviderItemViewModelTests
{
    [Test]
    public void WhenConvertingProvider_AndNoLastStartDate_ThenMapsOpenToNewStarts()
    {
        var provider = new ProviderCourseModel
        {
            Ukprn = 10000001,
            ProviderName = "ACORN SKILLS TRAINING",
            DateLastStarts = null
        };

        AllowedProviderItemViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
        model.DeliveryStatusDescription.Should().Be("Open to new starts");
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--green");
        model.HasLastStartDate.Should().BeFalse();
    }

    [Test]
    public void WhenConvertingProvider_AndFutureLastStartDate_ThenMapsLastStartDateAdded()
    {
        var date = DateTime.UtcNow.Date.AddDays(10);
        var provider = new ProviderCourseModel
        {
            Ukprn = 10019900,
            ProviderName = "BABINGTON LTD",
            DateLastStarts = date
        };

        AllowedProviderItemViewModel model = provider;

        model.DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
        model.DeliveryStatusTagClass.Should().Be("govuk-tag--orange");
        model.HasLastStartDate.Should().BeTrue();
        model.LastStartDateText.Should().Be(date.ToString("dd MMM yyyy"));
    }
}
