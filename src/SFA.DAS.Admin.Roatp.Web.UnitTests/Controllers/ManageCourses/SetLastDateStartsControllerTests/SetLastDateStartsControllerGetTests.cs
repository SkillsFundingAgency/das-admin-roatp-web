using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.SetLastDateStartsControllerTests;

[TestFixture]
public class SetLastDateStartsControllerGetTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private const int OtherUkprn = 10000001;

    [Test, MoqAutoData]
    public async Task WhenProviderHasLastDateStarts_ThenPrepopulatesDateFieldsAndIsChangeMode(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = lastDateStarts }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as SetLastDateStartsViewModel;
        using (new AssertionScope())
        {
            model!.Day.Should().Be("15");
            model.Month.Should().Be("03");
            model.Year.Should().Be("2027");
            model.IsChangingExistingDate.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenProviderExists_ThenReturnsView(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = "BP TRAINING", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as SetLastDateStartsViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(SetLastDateStartsController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.Ukprn.Should().Be(Ukprn);
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
            model.IsChangingExistingDate.Should().BeFalse();
            model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = OtherUkprn, ProviderName = "Other", LastDateStarts = null }
        ];

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(IUkprnAndLarsCodeValidator.LarsCode), "invalid")
            ]));

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        larsCodeServiceMock.Verify(
            s => s.GetCourseDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenCourseDetailsBecomeUnavailable_ThenReturnsNotFound(
        [Frozen] Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock,
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;

        SetupValidRoute(ukprnAndLarsCodeValidatorMock);
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidRoute(Mock<IValidator<IUkprnAndLarsCodeValidator>> ukprnAndLarsCodeValidatorMock)
    {
        ukprnAndLarsCodeValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IUkprnAndLarsCodeValidator>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }
}
