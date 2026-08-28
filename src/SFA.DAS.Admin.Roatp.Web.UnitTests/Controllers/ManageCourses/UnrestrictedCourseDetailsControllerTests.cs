using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class UnrestrictedCourseDetailsControllerTests
{
    private const string LarsCode = "105";
    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndCourseIsUnrestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Route = "Education and early years";
        response.LearningType = LearningType.Apprenticeship;
        response.IsCourseRestricted = false;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = 10019900,
                ProviderName = "BABINGTON LTD",
                LastDateStarts = null
            },
            new ProviderCourseModel
            {
                Ukprn = 10000001,
                ProviderName = "ACORN SKILLS TRAINING",
                LastDateStarts = null
            }
        ];

        SetupCourseResponse(outerApiClientMock, response);

        var result = await sut.Index(LarsCode, CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        result!.ViewName.Should().Be(UnrestrictedCourseDetailsController.ViewPath);
        var model = result.Model as UnrestrictedCourseDetailsViewModel;
        model.Should().NotBeNull();
        model!.DisplayTitle.Should().Be("Academic professional (Level 7)");
        model.Sector.Should().Be("Education and early years");
        model.LarsCode.Should().Be(LarsCode);
        model.StatusText.Should().Be("Unrestricted");
        model.HasProviders.Should().BeTrue();
        model.ProviderCountDescription.Should().Be("2 providers");
        model.Providers.Select(p => p.ProviderName).Should().ContainInOrder("ACORN SKILLS TRAINING", "BABINGTON LTD");
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndNoProviders_ThenReturnsEmptyState(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] UnrestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        response.Providers = [];

        SetupCourseResponse(outerApiClientMock, response);

        var result = await sut.Index(LarsCode, CancellationToken.None) as ViewResult;

        var model = result!.Model as UnrestrictedCourseDetailsViewModel;
        model!.HasNoProviders.Should().BeTrue();
        model.Providers.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndCourseIsRestricted_ThenRedirectsToRestrictedDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;

        SetupCourseResponse(outerApiClientMock, response);

        var result = await sut.Index(LarsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Once);
    }

    [TestCase(HttpStatusCode.BadRequest)]
    public async Task WhenGettingUnrestrictedCourseDetails_AndCourseApiReturnsBadRequest_ThenReturnsNotFound(
        HttpStatusCode statusCode)
    {
        var outerApiClientMock = new Mock<IOuterApiClient>();
        var sessionServiceMock = new Mock<ISessionService>();
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(statusCode), null, new RefitSettings(), null));

        var sut = new UnrestrictedCourseDetailsController(outerApiClientMock.Object, sessionServiceMock.Object);

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndCourseApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] UnrestrictedCourseDetailsController sut)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings(), null));

        var act = () => sut.Index(LarsCode, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage($"Failed to retrieve course details for LARS code '{LarsCode}'. Status code: InternalServerError.");
    }

    [Test, MoqAutoData]
    public void WhenPostingRestrictThisCourse_ThenStoresSessionAndRedirectsToConfirm(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut)
    {
        var submitModel = new RestrictCourseSubmitModel
        {
            LarsCode = LarsCode,
            DisplayName = "Academic professional (Level 7)"
        };

        var result = sut.Index(LarsCode, submitModel) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictCourseConfirm);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);
        sessionServiceMock.Verify(s => s.Set(
            SessionKeys.AddRestrictedCourse,
            It.Is<AddRestrictedCourseSessionModel>(m =>
                m.LarsCode == LarsCode &&
                m.DisplayName == "Academic professional (Level 7)")), Times.Once);
    }

    private static void SetupCourseResponse(
        Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));
    }
}
