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
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.AddAllowedCourseControllerTests;

[TestFixture]
public class AddAllowedCourseControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string ProviderName = "Denton Business Services Limited";
    private const string SelectedLarsCode = "105";
    private const string SelectedCourseTitle = "Electrical";
    private const int SelectedCourseLevel = 3;
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";

    [Test, MoqAutoData]
    public async Task WhenPostingAddAllowedCourse_AddsCourseSetsBanner_ThenRedirectsToAllowedCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<AddAllowedCourseSubmitModel>> validator,
        [Greedy] AddAllowedCourseController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.OK);
        validator.Setup(x => x.Validate(It.IsAny<AddAllowedCourseSubmitModel>()))
            .Returns(new ValidationResult());
        sut.SetupAuthenticatedUser();
        sut.AddTempData();

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new AddAllowedCourseSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey]
                .Should().Be(AddAllowedCourseController.GetSuccessBannerMessage("Electrical (Level 3)"));
        }

        outerApiClientMock.Verify(c => c.UpsertProviderAllowedCourse(
            Ukprn,
            SelectedLarsCode,
            It.Is<UpsertProviderAllowedCourseRequest>(r =>
                r.UserId == MockedUser.AuthenticatedUser.UserId()
                && r.UserDisplayName == MockedUser.AuthenticatedUser.UserDisplayName()
                && r.LastDateStarts == null
                && !r.IsStartRestricted),
            It.IsAny<CancellationToken>()), Times.Once);
        outerApiClientMock.Verify(
            c => c.GetCoursesToBeAllowed(It.IsAny<int>(), It.IsAny<CourseType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddAllowedCourse_AndNoCourseSelected_ThenReturnsViewWithValidationError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<AddAllowedCourseSubmitModel>> validator,
        [Greedy] AddAllowedCourseController sut)
    {
        SetupRestrictedProvider(ukprnServiceMock);
        SetupCoursesToBeAllowed(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<AddAllowedCourseSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(AddAllowedCourseSubmitModel.SelectedLarsCode),
                    AddAllowedCourseSubmitModelValidator.NoCourseSelectedErrorMessage)
            ]));
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new AddAllowedCourseSubmitModel(),
            CancellationToken.None) as ViewResult;
        var model = result?.Model as AddAllowedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(ProviderName);
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            model.Courses.Select(course => course.Value).Should().Equal(SelectedLarsCode);
            sut.ModelState.IsValid.Should().BeFalse();
        }

        coursesServiceMock.Verify(
            c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(
            c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddAllowedCourse_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<AddAllowedCourseSubmitModel>> validator,
        [Greedy] AddAllowedCourseController sut)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);
        validator.Setup(x => x.Validate(It.IsAny<AddAllowedCourseSubmitModel>()))
            .Returns(new ValidationResult());

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new AddAllowedCourseSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddAllowedCourse_AndApiReturnsNotFound_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<AddAllowedCourseSubmitModel>> validator,
        [Greedy] AddAllowedCourseController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupUpsertResponse(outerApiClientMock, HttpStatusCode.NotFound);
        validator.Setup(x => x.Validate(It.IsAny<AddAllowedCourseSubmitModel>()))
            .Returns(new ValidationResult());
        sut.SetupAuthenticatedUser();

        var act = () => sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new AddAllowedCourseSubmitModel { SelectedLarsCode = SelectedLarsCode },
            CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddAllowedCourse_AndNoCourseSelectedAndOrganisationIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IUkprnService> ukprnServiceMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddAllowedCourseSubmitModel>> validator,
        [Greedy] AddAllowedCourseController sut)
    {
        ukprnServiceMock
            .Setup(s => s.GetOrganisationAsync(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOrganisationResponse?)null);
        validator.Setup(x => x.Validate(It.IsAny<AddAllowedCourseSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(AddAllowedCourseSubmitModel.SelectedLarsCode),
                    AddAllowedCourseSubmitModelValidator.NoCourseSelectedErrorMessage)
            ]));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            new AddAllowedCourseSubmitModel(),
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.UpsertProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void SetupUrlHelper(AddAllowedCourseController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl);
    }

    private static void SetupRestrictedProvider(Mock<IUkprnService> ukprnServiceMock)
    {
        ukprnServiceMock.SetupOrganisationRestriction(
            Ukprn,
            isRestricted: true,
            CourseType.Apprenticeship,
            ProviderName);
    }

    private static void SetupGetCourse(Mock<ICoursesService> coursesServiceMock)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(SelectedLarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCourseResponse
            {
                LarsCode = SelectedLarsCode,
                Title = SelectedCourseTitle,
                Level = SelectedCourseLevel
            });
    }

    private static void SetupCoursesToBeAllowed(Mock<IOuterApiClient> outerApiClientMock)
    {
        outerApiClientMock
            .Setup(c => c.GetCoursesToBeAllowed(Ukprn, CourseType.Apprenticeship, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(
                HttpStatusCode.OK,
                new GetCoursesToBeAllowedResponse
                {
                    Courses =
                    [
                        new CourseToBeAllowedModel
                        {
                            LarsCode = SelectedLarsCode,
                            Title = SelectedCourseTitle,
                            Level = SelectedCourseLevel
                        }
                    ]
                }));
    }

    private static void SetupUpsertResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.UpsertProviderAllowedCourse(
                Ukprn,
                SelectedLarsCode,
                It.IsAny<UpsertProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<object>(statusCode, method: HttpMethod.Post));
    }
}
