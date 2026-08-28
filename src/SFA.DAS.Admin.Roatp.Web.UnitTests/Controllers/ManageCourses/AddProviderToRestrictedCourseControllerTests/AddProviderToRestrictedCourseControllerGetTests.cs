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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.AddProviderToRestrictedCourseControllerTests;

[TestFixture]
public class AddProviderToRestrictedCourseControllerGetTests
{
    private const string LarsCode = "105";
    private const string ProvidersSearchUrl = $"/restricted-courses/{LarsCode}/providers/search";

    [Test, MoqAutoData]
    public async Task WhenGettingAddProvider_AndCourseIsRestricted_ThenClearsSessionAndReturnsView(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, larsCodeServiceMock);
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.SearchProvidersForRestrictedCourse, ProvidersSearchUrl);

        var result = await sut.Index(LarsCode, CancellationToken.None) as ViewResult;

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AddProviderToRestrictedCourse), Times.Once);
        sessionServiceMock.Verify(
            s => s.Delete(SessionKeys.NotAllowedProvidersForRestrictedCourse(LarsCode)),
            Times.Once);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddProviderToRestrictedCourseController.ViewPath);
            var model = result.Model as AddProviderToRestrictedCourseViewModel;
            model.Should().NotBeNull();
            model!.LarsCode.Should().Be(LarsCode);
            model.Title.Should().Be("Academic professional");
            model.Level.Should().Be(7);
            model.DisplayTitle.Should().Be("Academic professional (Level 7)");
            model.ProvidersSearchUrl.Should().Be(ProvidersSearchUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddProvider_AndLarsCodeIsInvalid_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut)
    {
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddProvider_AndCourseIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupRestrictedCourse(
        GetRestrictedCourseDetailsResponse response,
        Mock<ILarsCodeService> larsCodeServiceMock)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.IsCourseRestricted = true;
        response.Providers = [];

        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}
