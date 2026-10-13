using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ConfirmRestrictProviderControllerTests;

[TestFixture]
public class ConfirmRestrictProviderControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_ThenRestrictsProviderSetsBannerAndRedirectsToAllowedCourses(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmRestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        sut.SetupAuthenticatedUser();
        sut.AddTempData();
        SetupRestrictResponse(outerApiClientMock, HttpStatusCode.OK);

        var result = await sut.IndexPost(Ukprn, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey]
                .Should().Be(ConfirmRestrictProviderController.SuccessBannerMessage);
        }

        outerApiClientMock.Verify(c => c.RestrictProvider(
            Ukprn,
            CourseType.Apprenticeship,
            It.Is<RestrictProviderRequest>(r =>
                r.UserId == MockedUser.AuthenticatedUser.UserId()
                && r.UserDisplayName == MockedUser.AuthenticatedUser.UserDisplayName()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_AndProviderNameIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ConfirmRestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<GetOrganisationResponse>(HttpStatusCode.NotFound, includeException: false));

        var result = await sut.IndexPost(Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(c => c.RestrictProvider(
            It.IsAny<int>(),
            It.IsAny<CourseType>(),
            It.IsAny<RestrictProviderRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingConfirm_AndApiReturnsNotFound_ThenThrows(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ConfirmRestrictProviderController sut)
    {
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        sut.SetupAuthenticatedUser();
        SetupRestrictResponse(outerApiClientMock, HttpStatusCode.NotFound);

        var act = () => sut.IndexPost(Ukprn, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        outerApiClientMock.Verify(c => c.RestrictProvider(
            Ukprn,
            CourseType.Apprenticeship,
            It.IsAny<RestrictProviderRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static void SetupRestrictResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.RestrictProvider(
                Ukprn,
                CourseType.Apprenticeship,
                It.IsAny<RestrictProviderRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<object>(statusCode, method: HttpMethod.Post));
    }
}
