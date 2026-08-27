using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.SetLastDateStartsControllerTests;

[TestFixture]
public class SetLastDateStartsControllerGetTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test, MoqAutoData]
    public async Task WhenProviderHasLastDateStarts_ThenPrepopulatesDateFieldsAndIsChangeMode(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = lastDateStarts
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseAndProvider(restrictedCourseProviderServiceMock, response, provider);

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
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = null
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseAndProvider(restrictedCourseProviderServiceMock, response, provider);

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
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((GetRestrictedCourseDetailsResponse, ProviderCourseModel)?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenRouteIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        restrictedCourseProviderServiceMock.Verify(
            s => s.GetCourseAndProviderAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenCourseDetailsBecomeUnavailable_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((GetRestrictedCourseDetailsResponse, ProviderCourseModel)?)null);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupValidRoute(Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static void SetupCourseAndProvider(
        Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        GetRestrictedCourseDetailsResponse response,
        ProviderCourseModel provider)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((response, provider));
    }
}
