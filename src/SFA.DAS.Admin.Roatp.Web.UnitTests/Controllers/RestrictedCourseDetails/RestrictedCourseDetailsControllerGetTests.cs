using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.RestrictedCourseDetails;

[TestFixture]
public class RestrictedCourseDetailsControllerGetTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndApiReturnsProviders_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode,
        string pageUrl,
        string backUrl,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = larsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Route = "Education and early years";
        response.LearningType = LearningType.Apprenticeship;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = 10019900,
                ProviderName = "BABINGTON LTD",
                DateLastStarts = DateTime.UtcNow.Date.AddDays(30)
            },
            new ProviderCourseModel
            {
                Ukprn = 10000001,
                ProviderName = "ACORN SKILLS TRAINING",
                DateLastStarts = null
            }
        ];

        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, backUrl)
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, pageUrl);

        var result = await sut.Index(larsCode, CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model.Should().NotBeNull();
        model!.DisplayTitle.Should().Be("Academic professional (Level 7)");
        model.Sector.Should().Be("Education and early years");
        model.LarsCode.Should().Be(larsCode);
        model.Level.Should().Be(7);
        model.StatusText.Should().Be("Restricted");
        model.HasProviders.Should().BeTrue();
        model.ProviderCount.Should().Be(2);
        model.ProviderCountDescription.Should().Be("2 providers");
        model.BackLinkUrl.Should().Be(backUrl);
        model.BackLinkText.Should().Be("Back to restricted courses");
        model.Providers.Select(p => p.ProviderName).Should().ContainInOrder("ACORN SKILLS TRAINING", "BABINGTON LTD");
        model.Providers.First().DeliveryStatus.Should().Be(DeliveryStatus.OpenToNewStarts);
        model.Providers.Last().DeliveryStatus.Should().Be(DeliveryStatus.LastStartDateAdded);
        model.Providers.Should().OnlyContain(p => p.ChangeUrl == pageUrl);

        outerApiClientMock.Verify(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndNoProviders_ThenReturnsEmptyState(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = larsCode;
        response.IsCourseRestricted = true;
        response.Providers = [];

        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(larsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourses, "/restricted-courses")
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, "/restricted-courses/105");

        var result = await sut.Index(larsCode, CancellationToken.None) as ViewResult;

        var model = result!.Model as RestrictedCourseDetailsViewModel;
        model!.HasNoProviders.Should().BeTrue();
        model.Providers.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedCourseDetails_AndApiFails_ThenRedirectsToRestrictedCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCourseDetailsController sut,
        string larsCode)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.Index(larsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourses);
    }
}
