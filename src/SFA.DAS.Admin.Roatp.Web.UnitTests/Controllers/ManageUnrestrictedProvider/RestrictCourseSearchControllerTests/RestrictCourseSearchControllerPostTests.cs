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
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RestrictCourseSearchControllerTests;

public class RestrictCourseSearchControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string SelectedLarsCode = "105";
    private const string SelectedCourseTitle = "Alpha course";
    private const int SelectedCourseLevel = 6;

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndCourseIsSelected_ThenStoresSessionAndRefreshesPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictCourseSearchSubmitModel>> validator,
        [Greedy] RestrictCourseSearchController controller)
    {
        SetupSearchableCourses(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<RestrictCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await controller.Index(
            Ukprn,
            new RestrictCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None) as ViewResult;
        var model = actual?.Model as RestrictCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(RestrictCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.SelectedLarsCode.Should().Be(SelectedLarsCode);
            model.Courses.Should().Contain(course => course.Value == SelectedLarsCode && course.Selected);
            controller.ModelState.IsValid.Should().BeTrue();
        }

        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.RestrictCourse,
            It.Is<RestrictCourseSessionModel>(m =>
                m.Ukprn == Ukprn &&
                m.LarsCode == SelectedLarsCode &&
                m.Title == SelectedCourseTitle &&
                m.Level == SelectedCourseLevel &&
                m.DisplayTitle == "Alpha course (Level 6)")), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndNoCourseIsSelected_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictCourseSearchSubmitModel>> validator,
        [Greedy] RestrictCourseSearchController controller)
    {
        SetupSearchableCourses(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<RestrictCourseSearchSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(RestrictCourseSearchSubmitModel.SelectedLarsCode),
                    RestrictCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage)
            ]));

        var actual = await controller.Index(
            Ukprn,
            new RestrictCourseSearchSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = actual?.Model as RestrictCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(RestrictCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Courses.Should().HaveCount(2);
            model.SelectedLarsCode.Should().BeNull();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictCourse, It.IsAny<RestrictCourseSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndSelectedCourseIsNotInList_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictCourseSearchSubmitModel>> validator,
        [Greedy] RestrictCourseSearchController controller)
    {
        SetupSearchableCourses(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<RestrictCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await controller.Index(
            Ukprn,
            new RestrictCourseSearchSubmitModel { SelectedLarsCode = "999" },
            CancellationToken.None);

        actual.Should().BeOfType<NotFoundResult>();

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictCourse, It.IsAny<RestrictCourseSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndCourseIsSelected_AndNotRestrictedApprenticeshipsAreNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictCourseSearchSubmitModel>> validator,
        [Greedy] RestrictCourseSearchController controller)
    {
        SetupNotRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.NotFound);
        validator.Setup(x => x.Validate(It.IsAny<RestrictCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await controller.Index(
            Ukprn,
            new RestrictCourseSearchSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        actual.Should().BeOfType<NotFoundResult>();

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictCourse, It.IsAny<RestrictCourseSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingRestrictCourseSearch_AndNoCourseIsSelected_AndNotRestrictedApprenticeshipsAreNotFound_ThenReloadsViewWithEmptyCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<RestrictCourseSearchSubmitModel>> validator,
        [Greedy] RestrictCourseSearchController controller)
    {
        SetupNotRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.NotFound);
        validator.Setup(x => x.Validate(It.IsAny<RestrictCourseSearchSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(RestrictCourseSearchSubmitModel.SelectedLarsCode),
                    RestrictCourseSearchSubmitModelValidator.NoCourseSelectedErrorMessage)
            ]));

        var actual = await controller.Index(
            Ukprn,
            new RestrictCourseSearchSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = actual?.Model as RestrictCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(RestrictCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Courses.Should().BeEmpty();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.RestrictCourse, It.IsAny<RestrictCourseSessionModel>()),
            Times.Never);
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
                        new RestrictedCourseModel
                        {
                            LarsCode = SelectedLarsCode,
                            Title = SelectedCourseTitle,
                            Level = SelectedCourseLevel
                        },
                        new RestrictedCourseModel
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
}
