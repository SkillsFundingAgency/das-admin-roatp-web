using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.CourseRestrictions.SetLastDateStartsControllerTests;

[TestFixture]
public class SetLastDateStartsControllerGetTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";

    [Test, MoqAutoData]
    public async Task WhenProviderHasLastDateStarts_ThenPrepopulatesDateFieldsAndIsChangeMode(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        var lastDateStarts = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = lastDateStarts
            }
        ];

        SetupCourse(outerApiClientMock, response);

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

        sessionServiceMock.Verify(
            s => s.Set(
                SessionKeys.SetLastDateStarts,
                It.Is<SetLastDateStartsSessionModel>(m =>
                    m.Ukprn == Ukprn
                    && m.LarsCode == LarsCode
                    && m.ProviderLastDateStarts == lastDateStarts)),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenProviderExists_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = null
            }
        ];

        SetupCourse(outerApiClientMock, response);

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

        sessionServiceMock.Verify(
            s => s.Set(
                SessionKeys.SetLastDateStarts,
                It.Is<SetLastDateStartsSessionModel>(m =>
                    m.Ukprn == Ukprn
                    && m.LarsCode == LarsCode
                    && m.ProviderName == "BP TRAINING"
                    && m.ProviderLastDateStarts == null)),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SetLastDateStartsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.Providers = [];
        SetupCourse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.SetLastDateStarts, It.IsAny<SetLastDateStartsSessionModel>()),
            Times.Never);
    }

    [Test]
    public async Task WhenCourseApiReturnsNotFound_ThenReturnsNotFound()
    {
        var outerApiClientMock = new Mock<IOuterApiClient>();
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var sut = new SetLastDateStartsController(
            outerApiClientMock.Object,
            Mock.Of<ISessionService>(),
            Mock.Of<IValidator<SetLastDateStartsSubmitModel>>());

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenCourseApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SetLastDateStartsController sut)
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            httpResponse,
            new RefitSettings());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                httpResponse, null, new RefitSettings(), apiException));

        var act = () => sut.Index(LarsCode, Ukprn, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
        sessionServiceMock.Verify(
            s => s.Set(SessionKeys.SetLastDateStarts, It.IsAny<SetLastDateStartsSessionModel>()),
            Times.Never);
    }

    private static void SetupCourse(
        Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));
    }
}
