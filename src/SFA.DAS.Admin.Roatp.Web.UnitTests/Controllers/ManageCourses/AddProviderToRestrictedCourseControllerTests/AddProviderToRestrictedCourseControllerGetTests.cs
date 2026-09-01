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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.AddProviderToRestrictedCourseControllerTests;

[TestFixture]
public class AddProviderToRestrictedCourseControllerGetTests
{
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task WhenGettingAddProvider_AndCourseIsRestricted_ThenClearsSessionAndReturnsView(
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, notAllowedProvidersServiceMock);

        var result = await sut.Index(LarsCode, CancellationToken.None) as ViewResult;

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
            model.Providers.Should().HaveCount(2);
            model.Providers.Select(p => p.Value).Should().Contain(["10000001", "10000002"]);
            model.Providers.Select(p => p.Text).Should().Contain("ALPHA TRAINING UKPRN: 10000001");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddProvider_AndCourseIsNotFound_ThenReturnsNotFound(
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut)
    {
        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRestrictedCourseDetailsResponse?)null);

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingAddProvider_AndCourseIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(LarsCode, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    private static void SetupRestrictedCourse(
        GetRestrictedCourseDetailsResponse response,
        Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = 10000002, ProviderName = "BETA TRAINING" },
            new ProviderCourseModel { Ukprn = 10000001, ProviderName = "ALPHA TRAINING" }
        ];

        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}
