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
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.AddAllowedCourseLastStartDateControllerTests;

[TestFixture]
public class AddAllowedCourseLastStartDateControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string CourseTitle = "Carpentry";
    private const int CourseLevel = 1;
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";
    private static readonly DateTime CourseLastDateStarts = new(2028, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime EnteredDate = new(2027, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndDateIsValid_ThenPatchesCourseSetsBannerAndRedirectsToList(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.OK);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        sut.SetupAuthenticatedUser();
        sut.AddTempData();

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey]
                .Should().Be(AddAllowedCourseLastStartDateController.GetSuccessBannerMessage("Carpentry (Level 1)"));
        }

        validatorMock.Verify(
            v => v.ValidateAsync(
                It.Is<SetLastDateStartsSubmitModel>(m =>
                    m.LarsCode == LarsCode
                    && m.CourseLastDateStarts == CourseLastDateStarts),
                It.IsAny<CancellationToken>()),
            Times.Once);
        outerApiClientMock.Verify(
            c => c.GetProviderAllowedCourse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        outerApiClientMock.Verify(c => c.PutProviderAllowedCourse(
            Ukprn,
            LarsCode,
            It.Is<PutProviderAllowedCourseRequest>(r => r.LastDateStarts == EnteredDate && r.UserId == sut.User.UserId() && r.UserDisplayName == sut.User.UserDisplayName()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndRouteLearningTypeIsApprenticeshipUnit_ThenRedirectsToAllowedCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock, CourseType.ShortCourse);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.OK);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        sut.SetupAuthenticatedUser();
        sut.AddTempData();

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderAllowedCourses);
            result.RouteValues!["routeLearningType"].Should().Be(RouteLearningType.ApprenticeshipUnit);
            sut.TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey]
                .Should().Be(AddAllowedCourseLastStartDateController.GetSuccessBannerMessage("Carpentry (Level 1)"));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndNoDateIsEntered_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupUrlHelper(sut);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(SetLastDateStartsSubmitModel.Day),
                    SetLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage)
            ]));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel(),
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be("Carpentry (Level 1)");
            model.CourseLastDateStarts.Should().Be(CourseLastDateStarts);
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            sut.ModelState.IsValid.Should().BeFalse();
        }

        outerApiClientMock.Verify(
            c => c.PutProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<PutProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndDateIsAfterCourseLastDateStarts_ThenReloadsViewWithLarsError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock)
    {
        SetupGetCourse(coursesServiceMock);
        var sut = new AddAllowedCourseLastStartDateController(
            outerApiClientMock.Object,
            Mock.Of<IUkprnService>(),
            coursesServiceMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "02", Month = "06", Year = "2028" },
            new CancellationToken()) as ViewResult;
        var model = result?.Model as AddAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.Day.Should().Be("02");
            model.Month.Should().Be("06");
            model.Year.Should().Be("2028");
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage ==
                    $"The latest start date for this course is {CourseLastDateStarts.ToDisplayString()}. It is set by LARS and cannot be changed. Your chosen last date for new starts must come on or before this.");
        }

        outerApiClientMock.Verify(
            c => c.PutProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<PutProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndDateIsBeforeMinimum_ThenReloadsViewWithMinimumDateError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock)
    {
        SetupGetCourse(coursesServiceMock);
        var sut = new AddAllowedCourseLastStartDateController(
            outerApiClientMock.Object,
            Mock.Of<IUkprnService>(),
            coursesServiceMock.Object,
            new SetLastDateStartsSubmitModelValidator());
        SetupUrlHelper(sut);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "31", Month = "08", Year = "2014" },
            new CancellationToken()) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddAllowedCourseLastStartDateController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            sut.ModelState[SetLastDateStartsSubmitModelValidator.DateFieldName]!
                .Errors.Should().ContainSingle(e =>
                    e.ErrorMessage == SetLastDateStartsSubmitModelValidator.DateMustBeAfterMinimumErrorMessage);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        outerApiClientMock.Verify(
            c => c.PutProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<PutProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndPatchReturnsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.NotFound);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        sut.SetupAuthenticatedUser();

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddLastStartDate_AndPatchReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<SetLastDateStartsSubmitModel>> validatorMock,
        [Greedy] AddAllowedCourseLastStartDateController sut)
    {
        SetupGetCourse(coursesServiceMock);
        SetupPutResponse(outerApiClientMock, HttpStatusCode.InternalServerError);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SetLastDateStartsSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        sut.SetupAuthenticatedUser();

        var act = () => sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new SetLastDateStartsSubmitModel { Day = "12", Month = "07", Year = "2027" },
            new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    private static void SetupUrlHelper(AddAllowedCourseLastStartDateController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl);
    }

    private static void SetupGetCourse(
        Mock<ICoursesService> coursesServiceMock,
        CourseType courseType = CourseType.Apprenticeship)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCourseResponse
            {
                LarsCode = LarsCode,
                Title = CourseTitle,
                Level = CourseLevel,
                CourseType = courseType,
                LastDateStarts = CourseLastDateStarts
            });
    }

    private static void SetupPutResponse(Mock<IOuterApiClient> outerApiClientMock, HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.PutProviderAllowedCourse(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<PutProviderAllowedCourseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create<object>(
                statusCode,
                method: HttpMethod.Put,
                includeException: statusCode != HttpStatusCode.NotFound));
    }
}
