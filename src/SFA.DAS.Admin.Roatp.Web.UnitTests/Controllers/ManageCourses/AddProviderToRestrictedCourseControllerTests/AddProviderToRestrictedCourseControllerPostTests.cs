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
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.AddProviderToRestrictedCourseControllerTests;

[TestFixture]
public class AddProviderToRestrictedCourseControllerPostTests
{
    private const string LarsCode = "105";
    private const string ProvidersSearchUrl = $"/restricted-courses/{LarsCode}/providers/search";

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, larsCodeServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AddProviderToRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(AddProviderToRestrictedCourseSubmitModel.SearchTerm),
                    "Enter a provider's name or UKPRN and select it from the list")
            ]));
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.SearchProvidersForRestrictedCourse, ProvidersSearchUrl);

        var result = await sut.Index(
            LarsCode,
            new AddProviderToRestrictedCourseSubmitModel(),
            CancellationToken.None) as ViewResult;

        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<AddProviderToRestrictedCourseSessionModel>()),
            Times.Never);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddProviderToRestrictedCourseController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            var model = result.Model as AddProviderToRestrictedCourseViewModel;
            model!.DisplayTitle.Should().Be("Academic professional (Level 7)");
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndProviderSelected_ThenRefreshesAddPage(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, larsCodeServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AddProviderToRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult());

        var submitModel = new AddProviderToRestrictedCourseSubmitModel
        {
            LegalName = "BP TRAINING",
            Ukprn = "10007938"
        };

        var result = await sut.Index(LarsCode, submitModel, CancellationToken.None) as RedirectToRouteResult;

        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<AddProviderToRestrictedCourseSessionModel>()),
            Times.Never);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.AddProviderToRestrictedCourse);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndCourseIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<ILarsCodeService> larsCodeServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        larsCodeServiceMock
            .Setup(s => s.GetCourseDetailsAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(
            LarsCode,
            new AddProviderToRestrictedCourseSubmitModel { LegalName = "BP TRAINING", Ukprn = "10007938" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<AddProviderToRestrictedCourseSessionModel>()),
            Times.Never);
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
