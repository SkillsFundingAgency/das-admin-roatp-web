using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RestrictedCourseChangeRestrictionControllerTests;

[TestFixture]
public class RestrictedCourseChangeRestrictionControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string ProviderName = "Denton Business Services Limited";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public void WhenPostingChange_AndNoOptionIsSelected_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictedCourseChangeRestrictionSubmitModel>> validatorMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<RestrictedCourseChangeRestrictionSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(RestrictedCourseChangeRestrictionSubmitModel.SelectedOption),
                    RestrictedCourseChangeRestrictionSubmitModelValidator.NoOptionSelectedErrorMessage)
            ]));
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = sut.Index(Ukprn, LarsCode, new RestrictedCourseChangeRestrictionSubmitModel()) as ViewResult;
        var model = result?.Model as RestrictedCourseChangeRestrictionViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedCourseChangeRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be(DisplayTitle);
            sut.ModelState.IsValid.Should().BeFalse();
        }

        VerifySessionNotSetAndApiNotCalled(outerApiClientMock, sessionServiceMock);
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndAddOrChangeIsSelected_ThenRedirectsToSetLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictedCourseChangeRestrictionSubmitModel>> validatorMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<RestrictedCourseChangeRestrictionSubmitModel>()))
            .Returns(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            }) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ChangeRestrictedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }

        VerifySessionNotSetAndApiNotCalled(outerApiClientMock, sessionServiceMock);
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndRemoveIsSelected_ThenRedirectsToRemoveRestriction(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictedCourseChangeRestrictionSubmitModel>> validatorMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<RestrictedCourseChangeRestrictionSubmitModel>()))
            .Returns(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.Remove
            }) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.RemoveRestrictedCourseRestriction);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }

        VerifySessionNotSetAndApiNotCalled(outerApiClientMock, sessionServiceMock);
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndSessionIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.RestrictedCourseChangeRestriction))
            .Returns((ChangeRestrictedCourseRestrictionSessionModel?)null);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            }) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        VerifySessionNotSetAndApiNotCalled(outerApiClientMock, sessionServiceMock);
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            }) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        VerifySessionNotSetAndApiNotCalled(outerApiClientMock, sessionServiceMock);
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndSessionLarsCodeDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupSession(sessionServiceMock, larsCode: "999");

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            }) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        VerifySessionNotSetAndApiNotCalled(outerApiClientMock, sessionServiceMock);
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
                ProviderName = ProviderName,
                CourseLastDateStarts = LastDateStarts
            });
    }

    private static void VerifySessionNotSetAndApiNotCalled(
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<ISessionService> sessionServiceMock)
    {
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictedCourseChangeRestriction, It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
