using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.RestrictedCourses;

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
            new ProviderCourseModel { Ukprn = 2, ProviderName = "Zebra Training", DateLastStarts = null },
            new ProviderCourseModel { Ukprn = 1, ProviderName = "Alpha Training", DateLastStarts = DateTime.UtcNow.Date.AddDays(-1) }
        ];

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

    [Test, MoqAutoData]
    public void WhenMappingFromResponse_AndNoProviders_ThenSetsEmptyStateFlags(
        GetRestrictedCourseDetailsResponse response)
    {
        response.IsCourseRestricted = true;
        response.Providers = [];

        RestrictedCourseDetailsViewModel model = response;

        model.HasProviders.Should().BeFalse();
        model.HasNoProviders.Should().BeTrue();
        model.ProviderCountDescription.Should().Be("0 providers");
    }
}
