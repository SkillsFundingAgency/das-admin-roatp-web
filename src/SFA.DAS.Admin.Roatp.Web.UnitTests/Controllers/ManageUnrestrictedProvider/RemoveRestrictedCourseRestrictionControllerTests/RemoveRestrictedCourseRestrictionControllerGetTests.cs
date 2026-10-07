using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RemoveRestrictedCourseRestrictionControllerTests;

[TestFixture]
public class RemoveRestrictedCourseRestrictionControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string DisplayTitle = "Electrical (Level 3)";
    private const string ProviderName = "Denton Business Services Limited";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveRestriction_AndSessionExists_ThenReturnsViewWithCourseDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>> validatorMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        SetupEligibility(validatorMock);
        sut.SetupHttpContext();
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(Ukprn, LarsCode) as ViewResult;
        var model = result?.Model as RemoveRestrictedCourseRestrictionViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RemoveRestrictedCourseRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.LarsCode.Should().Be(LarsCode);
            model.CourseDisplayTitle.Should().Be(DisplayTitle);
            model.CancelUrl.Should().Be(RestrictedCoursesUrl);
            model.Should().NotBeAssignableTo<IBackLink>();
        }

        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<ChangeRestrictedCourseRestrictionSessionModel>(m =>
                    m.Ukprn == Ukprn
                    && m.LarsCode == LarsCode),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveRestriction_AndSessionIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>> validatorMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        sessionServiceMock
            .Setup(s => s.Get<ChangeRestrictedCourseRestrictionSessionModel>(SessionKeys.RestrictedCourseChangeRestriction))
            .Returns((ChangeRestrictedCourseRestrictionSessionModel?)null);

        var result = await sut.Index(Ukprn, LarsCode) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        validatorMock.Verify(
            v => v.ValidateAsync(
                It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveRestriction_AndSessionUkprnDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock, ukprn: 99999999);

        var result = await sut.Index(Ukprn, LarsCode) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveRestriction_AndSessionLarsCodeDoesNotMatch_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock, larsCode: "999");

        var result = await sut.Index(Ukprn, LarsCode) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRemoveRestriction_AndEligibilityIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>> validatorMock,
        [Greedy] RemoveRestrictedCourseRestrictionController sut)
    {
        SetupSession(sessionServiceMock);
        SetupEligibility(validatorMock, isValid: false);
        sut.SetupHttpContext();

        var result = await sut.Index(Ukprn, LarsCode);

        result.Should().BeOfType<NotFoundResult>();
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
                CourseLastDateStarts = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified)
            });
    }

    private static void SetupEligibility(
        Mock<IValidator<ChangeRestrictedCourseRestrictionSessionModel>> validatorMock,
        bool isValid = true)
    {
        validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isValid
                ? new ValidationResult()
                : new ValidationResult([new ValidationFailure("Ukprn", "invalid")]));
    }
}
