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
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageRestrictedProvider.ChangeAllowedCourseLastStartDateControllerTests;

[TestFixture]
public class ChangeAllowedCourseLastStartDateControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string CourseTitle = "Carpentry";
    private const int CourseLevel = 1;
    private const string UnitCourseTitle = "Permanent modular building assembly - Apprenticeship unit";
    private const int UnitCourseLevel = 4;
    private const string AllowedCoursesUrl = "/providers/10019900/Apprenticeship/allowed-courses";
    private const string AllowedUnitsUrl = "/providers/10019900/ApprenticeshipUnit/allowed-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndNoOptionIsSelected_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ChangeAllowedCourseLastStartDateSubmitModel>> validatorMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts));
        SetupGetCourse(coursesServiceMock);
        SetupUrlHelper(sut);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeAllowedCourseLastStartDateSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ChangeAllowedCourseLastStartDateSubmitModel.SelectedOption),
                    ChangeAllowedCourseLastStartDateSubmitModelValidator.NoOptionSelectedErrorMessage)
            ]));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel(),
            new CancellationToken()) as ViewResult;
        var model = result?.Model as ChangeAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.CourseDisplayTitle.Should().Be("Carpentry (Level 1)");
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.CancelUrl.Should().Be(AllowedCoursesUrl);
            sut.ModelState.IsValid.Should().BeFalse();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndChangeIsSelected_ThenRedirectsToChangeLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ChangeAllowedCourseLastStartDateSubmitModel>> validatorMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts));
        SetupGetCourse(coursesServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeAllowedCourseLastStartDateSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Change
            },
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ChangeAllowedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndRemoveIsSelected_ThenRedirectsToChangeLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ChangeAllowedCourseLastStartDateSubmitModel>> validatorMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts));
        SetupGetCourse(coursesServiceMock);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeAllowedCourseLastStartDateSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Remove
            },
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ChangeAllowedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.Apprenticeship);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndNoOptionIsSelectedForApprenticeshipUnit_ThenReloadsViewWithError(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ChangeAllowedCourseLastStartDateSubmitModel>> validatorMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts, UnitCourseTitle, UnitCourseLevel));
        SetupGetCourse(coursesServiceMock, CourseType.ShortCourse, UnitCourseTitle, UnitCourseLevel);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedUnitsUrl);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeAllowedCourseLastStartDateSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ChangeAllowedCourseLastStartDateSubmitModel.SelectedOption),
                    ChangeAllowedCourseLastStartDateSubmitModelValidator.NoOptionSelectedErrorMessage)
            ]));

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel(),
            new CancellationToken()) as ViewResult;
        var model = result?.Model as ChangeAllowedCourseLastStartDateViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeAllowedCourseLastStartDateController.ViewPath);
            model.Should().NotBeNull();
            model!.RouteLearningType.Should().Be(RouteLearningType.ApprenticeshipUnit);
            model.CourseDisplayTitle.Should().Be("Permanent modular building assembly - Apprenticeship unit (Level 4)");
            model.LastDateStartsText.Should().Be("12 Jul 2026");
            model.CancelUrl.Should().Be(AllowedUnitsUrl);
            sut.ModelState.IsValid.Should().BeFalse();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndChangeIsSelectedForApprenticeshipUnit_ThenRedirectsToChangeLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ChangeAllowedCourseLastStartDateSubmitModel>> validatorMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts, UnitCourseTitle, UnitCourseLevel));
        SetupGetCourse(coursesServiceMock, CourseType.ShortCourse, UnitCourseTitle, UnitCourseLevel);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeAllowedCourseLastStartDateSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Change
            },
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ChangeAllowedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.ApprenticeshipUnit);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndRemoveIsSelectedForApprenticeshipUnit_ThenRedirectsToChangeLastStartDate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Frozen] Mock<IValidator<ChangeAllowedCourseLastStartDateSubmitModel>> validatorMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts, UnitCourseTitle, UnitCourseLevel));
        SetupGetCourse(coursesServiceMock, CourseType.ShortCourse, UnitCourseTitle, UnitCourseLevel);
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeAllowedCourseLastStartDateSubmitModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.ApprenticeshipUnit,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Remove
            },
            new CancellationToken()) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ChangeAllowedCourseLastStartDate);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
            result.RouteValues["routeLearningType"].Should().Be(RouteLearningType.ApprenticeshipUnit);
            result.RouteValues["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndAllowedCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, statusCode: HttpStatusCode.NotFound);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Change
            },
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
        coursesServiceMock.Verify(c => c.GetCourse(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ICoursesService> coursesServiceMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, CreateAllowedCourse(LastDateStarts));
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCourseResponse?)null);

        var result = await sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Change
            },
            new CancellationToken());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenPostingChangeLastStartDate_AndApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeAllowedCourseLastStartDateController sut)
    {
        SetupAllowedCourse(outerApiClientMock, statusCode: HttpStatusCode.InternalServerError);

        var act = () => sut.Index(
            Ukprn,
            RouteLearningType.Apprenticeship,
            LarsCode,
            new ChangeAllowedCourseLastStartDateSubmitModel
            {
                SelectedOption = ChangeAllowedCourseLastStartDateOptions.Change
            },
            new CancellationToken());

        await act.Should().ThrowAsync<ApiException>();
    }

    private static void SetupUrlHelper(ChangeAllowedCourseLastStartDateController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderAllowedCourses, AllowedCoursesUrl);
    }

    private static void SetupAllowedCourse(
        Mock<IOuterApiClient> outerApiClientMock,
        ProviderAllowedCourseModel? allowedCourse = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        outerApiClientMock
            .Setup(c => c.GetProviderAllowedCourse(Ukprn, LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OuterApiResponse.Create(statusCode, allowedCourse));
    }

    private static void SetupGetCourse(
        Mock<ICoursesService> coursesServiceMock,
        CourseType courseType = CourseType.Apprenticeship,
        string title = CourseTitle,
        int level = CourseLevel)
    {
        coursesServiceMock
            .Setup(c => c.GetCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCourseResponse
            {
                LarsCode = LarsCode,
                Title = title,
                Level = level,
                CourseType = courseType
            });
    }

    private static ProviderAllowedCourseModel CreateAllowedCourse(
        DateTime? lastDateStarts = null,
        string title = CourseTitle,
        int level = CourseLevel)
        => new()
        {
            LarsCode = LarsCode,
            Title = title,
            Level = level,
            LastDateStarts = lastDateStarts,
            IsClosedToNewStarts = false
        };
}
