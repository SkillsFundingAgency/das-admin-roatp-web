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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ProviderRestrictedCourseSearchControllerTests;

public class ProviderRestrictedCourseSearchControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string SelectedLarsCode = "105";
    private const string SelectedCourseTitle = "Alpha course";
    private const int SelectedCourseLevel = 6;
    private const string ProviderName = "Denton Business Services Limited";

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndProviderCourseIsNotFound_ThenStoresSessionAndRedirectsToConfirm(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupOrganisation(outerApiClientMock, sessionServiceMock);
        SetupProviderCourse(outerApiClientMock, HttpStatusCode.NotFound);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.RouteName.Should().Be(RouteNames.ConfirmProviderRestrictedCourse);
            actual.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderRestrictedCourse,
            It.Is<ProviderRestrictedCourseSessionModel>(m =>
                m.Ukprn == Ukprn &&
                m.LarsCode == SelectedLarsCode &&
                m.CourseDisplayTitle == "Alpha course (Level 6)" &&
                m.ProviderName == ProviderName)), Times.Once);
        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderNames,
            It.Is<Dictionary<int, string>>(d => d[Ukprn] == ProviderName)), Times.Once);
        coursesServiceMock.Verify(
            c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(Ukprn, SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetNotRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndProviderCourseIsFound_ThenStoresSessionAndRedirectsToSetLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupOrganisation(outerApiClientMock, sessionServiceMock);
        SetupProviderCourse(outerApiClientMock, HttpStatusCode.OK);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.RouteName.Should().Be(RouteNames.AddRestrictedCourseLastStartDate);
            actual.RouteValues!["ukprn"].Should().Be(Ukprn);
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderRestrictedCourse,
            It.Is<ProviderRestrictedCourseSessionModel>(m =>
                m.Ukprn == Ukprn &&
                m.LarsCode == SelectedLarsCode &&
                m.CourseDisplayTitle == "Alpha course (Level 6)" &&
                m.ProviderName == ProviderName)), Times.Once);
        coursesServiceMock.Verify(
            c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(Ukprn, SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetNotRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndGetProviderCourseReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupOrganisation(outerApiClientMock, sessionServiceMock);
        SetupProviderCourse(outerApiClientMock, HttpStatusCode.InternalServerError);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var act = () => sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderRestrictedCourse,
            It.IsAny<ProviderRestrictedCourseSessionModel>()), Times.Once);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndGetOrganisationReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupOrganisation(outerApiClientMock, sessionServiceMock, statusCode: HttpStatusCode.InternalServerError);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var act = () => sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.ProviderRestrictedCourse,
            It.IsAny<ProviderRestrictedCourseSessionModel>()), Times.Never);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndNoCourseIsSelected_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupSearchableCourses(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ProviderRestrictedCourseSearchSubmitModel.SelectedLarsCode),
                    ProviderRestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage)
            ]));

        var actual = await sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = actual?.Model as ProviderRestrictedCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(ProviderRestrictedCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Courses.Should().HaveCount(2);
            model.SelectedLarsCode.Should().BeNull();
            sut.ModelState.IsValid.Should().BeFalse();
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourse, It.IsAny<ProviderRestrictedCourseSessionModel>()),
            Times.Never);
        coursesServiceMock.Verify(
            c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndGetCourseThrowsNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourseThrows(coursesServiceMock, HttpStatusCode.NotFound);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var act = () => sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourse, It.IsAny<ProviderRestrictedCourseSessionModel>()),
            Times.Never);
        coursesServiceMock.Verify(
            c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetNotRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndSelectedCourseIsNotInCoursesList_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourse(coursesServiceMock, includeSelectedCourse: false);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        actual.Should().BeOfType<NotFoundResult>();

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourse, It.IsAny<ProviderRestrictedCourseSessionModel>()),
            Times.Never);
        coursesServiceMock.Verify(
            c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndGetCourseThrowsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupGetCourseThrows(coursesServiceMock, HttpStatusCode.InternalServerError);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var act = () => sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourse, It.IsAny<ProviderRestrictedCourseSessionModel>()),
            Times.Never);
        coursesServiceMock.Verify(
            c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut,
        GetOrganisationResponse organisationResponse)
    {
        SetupGetCourse(coursesServiceMock);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());
        SetupOrganisation(outerApiClientMock, sessionServiceMock, organisationResponse, HttpStatusCode.NotFound);

        var actual = await sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        actual.Should().BeOfType<NotFoundResult>();

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourse, It.IsAny<ProviderRestrictedCourseSessionModel>()),
            Times.Never);
        coursesServiceMock.Verify(
            c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndNoCourseIsSelected_AndNotRestrictedApprenticeshipsReturnsNotFound_ThenReloadsViewWithEmptyCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ProviderRestrictedCourseSearchSubmitModel>> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupNotRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.NotFound);
        validator.Setup(x => x.Validate(It.IsAny<ProviderRestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ProviderRestrictedCourseSearchSubmitModel.SelectedLarsCode),
                    ProviderRestrictedCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage)
            ]));

        var actual = await sut.Index(
            Ukprn,
            new ProviderRestrictedCourseSearchSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = actual?.Model as ProviderRestrictedCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(ProviderRestrictedCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Courses.Should().BeEmpty();
            sut.ModelState.IsValid.Should().BeFalse();
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.ProviderRestrictedCourse, It.IsAny<ProviderRestrictedCourseSessionModel>()),
            Times.Never);
        coursesServiceMock.Verify(
            c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetProviderCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void SetupGetCourse(
        Mock<ICoursesService> coursesServiceMock,
        bool includeSelectedCourse = true)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(includeSelectedCourse
                ? new GetCourseResponse
                {
                    LarsCode = SelectedLarsCode,
                    Title = SelectedCourseTitle,
                    Level = SelectedCourseLevel
                }
                : null);
    }

    private static void SetupGetCourseThrows(
        Mock<ICoursesService> coursesServiceMock,
        HttpStatusCode statusCode)
    {
        var apiException = ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(statusCode),
            new RefitSettings()).GetAwaiter().GetResult();

        coursesServiceMock
            .Setup(c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);
    }

    private static void SetupSearchableCourses(Mock<IOuterApiClient> outerApiClientMock)
    {
        outerApiClientMock
            .Setup(c => c.GetNotRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetNotRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                new GetNotRestrictedApprenticeshipsResponse
                {
                    Courses =
                    [
                        new NotRestrictedApprenticeshipModel
                        {
                            LarsCode = SelectedLarsCode,
                            Title = SelectedCourseTitle,
                            Level = SelectedCourseLevel
                        },
                        new NotRestrictedApprenticeshipModel
                        {
                            LarsCode = "300",
                            Title = "Beta course",
                            Level = 4
                        }
                    ]
                },
                new RefitSettings(),
                null));
    }

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<ISessionService> sessionServiceMock,
        GetOrganisationResponse? organisationResponse = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        sessionServiceMock.SetupProviderNameMissing();
        organisationResponse ??= new GetOrganisationResponse { LegalName = ProviderName };
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.GetOrganisation(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                httpResponse,
                organisationResponse,
                new RefitSettings(),
                apiException));
    }

    private static void SetupNotRestrictedApprenticeships(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.GetNotRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetNotRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(statusCode),
                null,
                new RefitSettings(),
                null));
    }

    private static void SetupProviderCourse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.NotFound)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        outerApiClientMock
            .Setup(c => c.GetProviderCourse(Ukprn, SelectedLarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(
                httpResponse,
                null,
                new RefitSettings(),
                apiException));
    }
}
