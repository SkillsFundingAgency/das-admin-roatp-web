using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

[TestFixture]
public class UnrestrictedCourseDetailsControllerTests
{
    private const string LarsCode = "105";
    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndCourseIsUnrestricted_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
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

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

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
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        response.Providers = [];

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(LarsCode, CancellationToken.None) as ViewResult;

        var model = result!.Model as UnrestrictedCourseDetailsViewModel;
        model!.HasNoProviders.Should().BeTrue();
        model.Providers.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndCourseIsRestricted_ThenRedirectsToRestrictedDetails(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = true;

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(LarsCode, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.RestrictedCourseDetails);
        result.RouteValues!["larsCode"].Should().Be(LarsCode);
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseDetails_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCourseDetailsController sut)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddRestrictedCourse), Times.Once);
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
}
