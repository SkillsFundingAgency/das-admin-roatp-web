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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.AddProviderToRestrictedCourseControllerTests;

[TestFixture]
public class AddProviderToRestrictedCourseControllerPostTests
{
    private const string LarsCode = "105";

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, notAllowedProvidersServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AddProviderToRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(AddProviderToRestrictedCourseSubmitModel.SelectedUkprn),
                    "Start typing the provider's name or UKPRN in the box")
            ]));

        var result = await sut.Index(
            LarsCode,
            new AddProviderToRestrictedCourseSubmitModel(),
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(AddProviderToRestrictedCourseController.ViewPath);
            sut.ModelState.IsValid.Should().BeFalse();
            var model = result.Model as AddProviderToRestrictedCourseViewModel;
            model!.DisplayTitle.Should().Be("Academic professional (Level 7)");
            model.Providers.Should().HaveCount(1);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndProviderSelected_ThenRefreshesAddPage(
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, notAllowedProvidersServiceMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AddProviderToRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult());

        var submitModel = new AddProviderToRestrictedCourseSubmitModel
        {
            SelectedUkprn = "10007938"
        };

        var result = await sut.Index(LarsCode, submitModel, CancellationToken.None) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.AddProviderToRestrictedCourse);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndCourseIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<INotAllowedProvidersService> notAllowedProvidersServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(
            LarsCode,
            new AddProviderToRestrictedCourseSubmitModel { SelectedUkprn = "10007938" },
            CancellationToken.None);

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
            new ProviderCourseModel { Ukprn = 10007938, ProviderName = "BP TRAINING" }
        ];

        notAllowedProvidersServiceMock
            .Setup(s => s.GetNotAllowedProvidersAsync(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }
}
