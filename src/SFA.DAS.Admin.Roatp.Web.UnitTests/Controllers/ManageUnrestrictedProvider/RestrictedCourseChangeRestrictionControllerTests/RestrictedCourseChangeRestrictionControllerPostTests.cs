using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
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
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenPostingChange_AndNoOptionIsSelected_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictedCourseChangeRestrictionSubmitModel>> validatorMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock);
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
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

        var result = await sut.Index(Ukprn, LarsCode, new RestrictedCourseChangeRestrictionSubmitModel(), CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedCourseChangeRestrictionViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedCourseChangeRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be("Electrical (Level 3)");
            sut.ModelState.IsValid.Should().BeFalse();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChange_AndAddOrChangeIsSelected_ThenStoresSessionAndRedirectsToSetLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictedCourseChangeRestrictionSubmitModel>> validatorMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock);
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<RestrictedCourseChangeRestrictionSubmitModel>()))
            .Returns(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ChangeRestrictedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }

        sessionServiceMock.Verify(
            s => s.Set(
                SessionKeys.RestrictedCourseChangeRestriction,
                It.Is<ChangeRestrictedCourseRestrictionSessionModel>(m =>
                    m.Ukprn == Ukprn
                    && m.LarsCode == LarsCode
                    && m.ProviderName == ProviderName
                    && m.CourseLastDateStarts == LastDateStarts)),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChange_AndRemoveIsSelected_ThenRefreshesPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictedCourseChangeRestrictionSubmitModel>> validatorMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock);
        sessionServiceMock.SetupProviderName(Ukprn, ProviderName);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<RestrictedCourseChangeRestrictionSubmitModel>()))
            .Returns(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedApprenticeships, RestrictedCoursesUrl);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.Remove
            },
            CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedCourseChangeRestrictionViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedCourseChangeRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.SelectedOption.Should().Be(RestrictedCourseChangeRestrictionOptions.Remove);
            sut.ModelState.IsValid.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChange_AndRestrictedApprenticeshipsAreNotFound_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock, HttpStatusCode.NotFound);

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedApprenticeships);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictedCourseChangeRestriction, It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChange_AndProviderNameIsMissing_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RestrictedCourseChangeRestrictionController sut)
    {
        SetupRestrictedApprenticeship(outerApiClientMock);
        sessionServiceMock.SetupProviderNameMissing();
        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound),
                null,
                new RefitSettings(),
                null));

        var result = await sut.Index(
            Ukprn,
            LarsCode,
            new RestrictedCourseChangeRestrictionSubmitModel
            {
                SelectedOption = RestrictedCourseChangeRestrictionOptions.AddOrChange
            },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictedCourseChangeRestriction, It.IsAny<ChangeRestrictedCourseRestrictionSessionModel>()),
            Times.Never);
    }

    private static void SetupRestrictedApprenticeship(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var courses = statusCode == HttpStatusCode.OK
            ? new List<ProviderRestrictedApprenticeshipModel>
            {
                new()
                {
                    LarsCode = LarsCode,
                    Title = "Electrical",
                    Level = 3,
                    LastDateStarts = LastDateStarts,
                    IsClosedToNewStarts = false
                }
            }
            : [];

        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(statusCode),
                new GetRestrictedApprenticeshipsResponse { Courses = courses },
                new RefitSettings(),
                null));
    }
}
