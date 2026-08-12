using FluentAssertions;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Models.ManageCourses;

[TestFixture]
public class UnrestrictedCourseDetailsViewModelTests
{
    [Test, MoqAutoData]
    public void WhenMappingFromResponse_AndProvidersExist_ThenMapsCourseDetailsAndSortsProviders(
        GetRestrictedCourseDetailsResponse response)
    {
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Route = "Education and early years";
        response.LearningType = LearningType.Apprenticeship;
        response.IsCourseRestricted = false;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 2, ProviderName = "Zebra Training", LastDateStarts = null },
            new ProviderCourseModel { Ukprn = 1, ProviderName = "Alpha Training", LastDateStarts = null }
        ];

        UnrestrictedCourseDetailsViewModel model = response;

        model.DisplayTitle.Should().Be("Academic professional (Level 7)");
        model.LearningTypeDescription.Should().Be("Apprenticeship");
        model.Sector.Should().Be("Education and early years");
        model.StatusText.Should().Be("Unrestricted");
        model.ProviderCountDescription.Should().Be("2 providers");
        model.HasProviders.Should().BeTrue();
        model.Providers.Select(p => p.ProviderName).Should().ContainInOrder("Alpha Training", "Zebra Training");
    }

    [Test, MoqAutoData]
    public void WhenMappingFromResponse_AndNoProviders_ThenSetsEmptyStateFlags(
        GetRestrictedCourseDetailsResponse response)
    {
        response.IsCourseRestricted = false;
        response.Providers = [];

        UnrestrictedCourseDetailsViewModel model = response;

        model.HasProviders.Should().BeFalse();
        model.HasNoProviders.Should().BeTrue();
        model.ProviderCountDescription.Should().Be("0 providers");
    }
}
