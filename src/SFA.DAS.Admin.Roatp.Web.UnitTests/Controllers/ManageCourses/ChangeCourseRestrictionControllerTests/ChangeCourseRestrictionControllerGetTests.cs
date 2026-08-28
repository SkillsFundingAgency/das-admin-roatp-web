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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.ChangeCourseRestrictionControllerTests;

[TestFixture]
public class ChangeCourseRestrictionControllerGetTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private static readonly DateTime LastDateStarts = new(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenProviderHasLastDateStarts_ThenReturnsView(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupValidRoute(restrictedCourseProviderServiceMock);
        SetupCourseWithLastDateStarts(restrictedCourseProviderServiceMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as ChangeCourseRestrictionViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeCourseRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.Ukprn.Should().Be(Ukprn);
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("01 Jun 2027");
            model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = null
        };

        SetupValidRoute(restrictedCourseProviderServiceMock);
        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((response, provider));

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        [Greedy] ChangeCourseRestrictionController sut)
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
        [Greedy] ChangeCourseRestrictionController sut)
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
        [Greedy] ChangeCourseRestrictionController sut)
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

    [Test]
    public void ChangeCourseRestrictionOptions_ExposeExpectedValues()
    {
        using (new AssertionScope())
        {
            ChangeCourseRestrictionOptions.Change.Should().Be("Change");
            ChangeCourseRestrictionOptions.Remove.Should().Be("Remove");
        }
    }

    private static void SetupValidRoute(Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock)
    {
        restrictedCourseProviderServiceMock
            .Setup(s => s.IsRouteValidAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static void SetupCourseWithLastDateStarts(
        Mock<IRestrictedCourseProviderService> restrictedCourseProviderServiceMock,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        var provider = new ProviderCourseModel
        {
            Ukprn = Ukprn,
            ProviderName = "BP TRAINING",
            LastDateStarts = LastDateStarts
        };

        restrictedCourseProviderServiceMock
            .Setup(s => s.GetCourseAndProviderAsync(LarsCode, Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((response, provider));
    }
}
