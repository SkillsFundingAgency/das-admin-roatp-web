using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RemoveRestrictedCourseRestrictionControllerTests;

[TestFixture]
public class RemoveRestrictedCourseRestrictionControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string ProviderName = "Denton Business Services Limited";

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveRestriction_ThenRemovesCourseDeletesSessionSetsBannerAndRedirectsToList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>> validatorMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        sut.SetupAuthenticatedUser();
        sut.AddTempData();
        SetupRemoveResponse(outerApiClientMock, HttpStatusCode.OK);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            sut.TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey]
                .Should().Be(RemoveRestrictedCourseRestrictionController.GetSuccessBannerMessage(DisplayTitle));
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Once);
        validatorMock.Verify(
            v => v.ValidateAsync(
                It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.RemoveRestrictedApprenticeship(
            Ukprn,
            LarsCode,
            It.Is<RemoveRestrictedApprenticeshipRequest>(r =>
                r.UserId == MockedUser.AuthenticatedUser.UserId()
                && r.UserDisplayName == MockedUser.AuthenticatedUser.UserDisplayName()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveRestriction_AndSessionIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.RestrictedCourseChangeRestriction))
            .Returns((ChangeRestrictedCourseRestrictionSessionModel?)null);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.RemoveRestrictedApprenticeship(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<RemoveRestrictedApprenticeshipRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveRestriction_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.RemoveRestrictedApprenticeship(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<RemoveRestrictedApprenticeshipRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveRestriction_AndSessionLarsCodeDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock, larsCode: "999");

        var result = await sut.Index(Ukprn, LarsCode, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        outerApiClientMock.Verify(c => c.RemoveRestrictedApprenticeship(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<RemoveRestrictedApprenticeshipRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRemoveRestriction_AndApiReturnsNotFound_ThenThrows(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        sut.SetupAuthenticatedUser();
        SetupRemoveResponse(outerApiClientMock, HttpStatusCode.NotFound);

        var act = () => sut.Index(Ukprn, LarsCode, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task WhenPostingRemoveRestriction_AndApiReturnsUnexpectedError_ThenThrows(HttpStatusCode statusCode)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var validatorMock = new Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>>();
        SetupSession(sessionServiceMock);
        SetupRemoveResponse(outerApiClientMock, statusCode);

        var sut = new RemoveRestrictedCourseRestrictionController(
            sessionServiceMock.Object,
            outerApiClientMock.Object,
            validatorMock.Object);
        sut.SetupAuthenticatedUser();

        var act = () => sut.Index(Ukprn, LarsCode, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictedCourseChangeRestriction), Times.Never);
    }

    private static void SetupSession(
        Mock<ISessionService> sessionServiceMock,
        int ukprn = Ukprn,
        string larsCode = LarsCode)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.RestrictedCourseChangeRestriction))
            .Returns(new ChangeRestrictedCourseRestrictionSessionModel
            {
                Ukprn = ukprn,
                LarsCode = larsCode,
                CourseDisplayTitle = DisplayTitle,
                ProviderName = ProviderName
            });
    }

    private static void SetupRemoveResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.RemoveRestrictedApprenticeship(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<RemoveRestrictedApprenticeshipRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<object>(statusCode, method: HttpMethod.Post));
    }
}
