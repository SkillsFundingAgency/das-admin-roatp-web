using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.AddProviderToRestrictedCourseControllerTests;

[TestFixture]
public class AddProviderToRestrictedCourseControllerPostTests
{
    private const string LarsCode = "105";
    private const int Ukprn = 10007938;
    private const string LegalName = "BP TRAINING";

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndValidationFails_ThenReturnsViewWithErrors(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, outerApiClientMock);
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
            model.Providers.Should().HaveCount(1);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndProviderSelected_ThenStoresSessionAndRedirects(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, outerApiClientMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AddProviderToRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult());

        var submitModel = new AddProviderToRestrictedCourseSubmitModel
        {
            SelectedUkprn = Ukprn.ToString()
        };

        var result = await sut.Index(LarsCode, submitModel, CancellationToken.None) as RedirectToRouteResult;

        sessionServiceMock.Verify(
            s => s.Set(
                SessionKeys.AddProviderToRestrictedCourse,
                It.Is<AddProviderToRestrictedCourseSessionModel>(m =>
                    m.LarsCode == LarsCode
                    && m.CourseDisplayTitle == "Academic professional (Level 7)"
                    && m.Ukprn == Ukprn
                    && m.LegalName == LegalName)),
            Times.Once);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.AddProviderToRestrictedCourse);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndProviderNotInList_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddProviderToRestrictedCourseSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupRestrictedCourse(response, outerApiClientMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AddProviderToRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult());

        var result = await sut.Index(
            LarsCode,
            new AddProviderToRestrictedCourseSubmitModel { SelectedUkprn = "not-a-provider" },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<AddProviderToRestrictedCourseSessionModel>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingAddProvider_AndCourseIsUnrestricted_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddProviderToRestrictedCourseController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.IsCourseRestricted = false;
        outerApiClientMock
            .Setup(c => c.GetProvidersRestrictedForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));

        var result = await sut.Index(
            LarsCode,
            new AddProviderToRestrictedCourseSubmitModel { SelectedUkprn = Ukprn.ToString() },
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        sessionServiceMock.Verify(
            s => s.Set(It.IsAny<string>(), It.IsAny<AddProviderToRestrictedCourseSessionModel>()),
            Times.Never);
    }

    private static void SetupRestrictedCourse(
        GetRestrictedCourseDetailsResponse response,
        Mock<IOuterApiClient> outerApiClientMock)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.IsCourseRestricted = true;
        response.Providers =
        [
            new ProviderCourseModel { Ukprn = Ukprn, ProviderName = LegalName }
        ];

        outerApiClientMock
            .Setup(c => c.GetProvidersRestrictedForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));
    }
}
