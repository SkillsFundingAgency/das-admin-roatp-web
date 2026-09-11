using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ApprenticeshipUnitsUpdateControllerTests;

public class ApprenticeshipUnitsUpdateControllerPostTests
{
    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndNoMatchingDetails_ThenRedirectsToHome(
      [Frozen] Mock<IOuterApiClient> outerApiClientMock,
      [Greedy] ApprenticeshipUnitsUpdateController sut,
      ApprenticeshipUnitsUpdateViewModel viewModel,
      int ukprn,
      CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.IsAny<UpdateCourseTypesModel>(), cancellationToken), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndNoChange_ThenRedirectsToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitsUpdateViewModel viewModel,
        bool containsApprenticeshipUnits,
        int ukprn,
        CancellationToken cancellationToken)
    {
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        var courseTypes = new List<AllowedCourseType>
        {
            new() { CourseType = CourseType.Apprenticeship }
        };

        if (containsApprenticeshipUnits)
        {
            courseTypes.Add(new() { CourseType = CourseType.ShortCourse });
        }

        getOrganisationResponse.AllowedCourseTypes = courseTypes;

        viewModel.ApprenticeshipUnitsSelectionId = containsApprenticeshipUnits;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.IsAny<UpdateCourseTypesModel>(), cancellationToken), Times.Never);
    }

    [Test]
    [MoqInlineAutoData(true, false, 2)]
    [MoqInlineAutoData(true, true, 1)]
    [MoqInlineAutoData(false, false, 1)]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndChangePostedSuccessfully_ThenPutsCourseTypes(
       bool isStandardCourseTypePresent,
       bool isShortCourseTypePresent,
       int expectedPutCourseTypes,
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Frozen] Mock<ISessionService> sessionServiceMock,
       [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
       [Greedy] ApprenticeshipUnitsUpdateController sut,
       GetOrganisationResponse getOrganisationResponse,
       ApprenticeshipUnitsUpdateViewModel viewModel,
       int ukprn,
       CancellationToken cancellationToken)
    {
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };
        var courseTypes = new List<AllowedCourseType>();
        if (isStandardCourseTypePresent) courseTypes.Add(new AllowedCourseType { CourseType = CourseType.Apprenticeship });
        if (isShortCourseTypePresent) courseTypes.Add(new AllowedCourseType { CourseType = CourseType.ShortCourse });

        getOrganisationResponse.AllowedCourseTypes = courseTypes;
        var currentCourseTypeIds = getOrganisationResponse.AllowedCourseTypes
            .Select(a => (int)a.CourseType).ToList();

        viewModel.ApprenticeshipUnitsSelectionId = !isShortCourseTypePresent;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));


        var expectedCourseTypeIds = currentCourseTypeIds;
        if (isShortCourseTypePresent)
        {
            expectedCourseTypeIds = currentCourseTypeIds.Where(a => a != (int)CourseType.ShortCourse).ToList();
        }
        else
        {
            expectedCourseTypeIds.Add((int)CourseType.ShortCourse);
        }

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        expectedCourseTypeIds.Count.Should().Be(expectedPutCourseTypes);

        switch (expectedPutCourseTypes)
        {
            case 1:
                outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.Is<UpdateCourseTypesModel>(
                    c => c.CourseTypeIds.Count == 1
                            && c.CourseTypeIds[0] == expectedCourseTypeIds[0]
                ), cancellationToken), Times.Once);
                break;
            case 2:
                outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.Is<UpdateCourseTypesModel>(
                    c => c.CourseTypeIds.Count == 2
                                && c.CourseTypeIds[0] == expectedCourseTypeIds[0]
                                && c.CourseTypeIds[1] == expectedCourseTypeIds[1]
                ), cancellationToken), Times.Once);
                break;
        }
    }

    [Test]
    [MoqInlineAutoData]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndValidationTriggered_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitsUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var selectionChoice = false;
        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };
        var courseTypes = new List<AllowedCourseType> { new() { CourseType = CourseType.ShortCourse } };

        getOrganisationResponse.AllowedCourseTypes = courseTypes;
        var currentCourseTypeIds = getOrganisationResponse.AllowedCourseTypes
            .Select(a => (int)a.CourseType).ToList();

        viewModel.ApprenticeshipUnitsSelectionId = selectionChoice;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as ViewResult;
        result.Should().NotBeNull();
        var model = result!.Model as ApprenticeshipUnitsUpdateViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Employer)]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndSupportingToOtherWithApprenticeshipsAndNoUnits_ThenRedirectsToProviderSummary(
        ProviderType providerTypeChangedTo,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationPatchService> patchServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitsUpdateViewModel viewModel,
        UpdateProviderTypeCourseTypesSessionModel sessionModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var courseTypesWithApprentices = new List<int> { (int)CourseType.Apprenticeship };
        sessionModel.CourseTypeIds = courseTypesWithApprentices;
        sessionModel.ProviderType = providerTypeChangedTo;

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        var apprenticeshipUnitsSelected = false;

        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };

        viewModel.ApprenticeshipUnitsSelectionId = apprenticeshipUnitsSelected;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn,
            It.Is<UpdateCourseTypesModel>(
                c => c.CourseTypeIds.Count == sessionModel.CourseTypeIds.Count
                && c.CourseTypeIds[0] == sessionModel.CourseTypeIds[0]
                )
            , cancellationToken), Times.Once);

        patchServiceMock.Verify(p => p.OrganisationPatched(ukprn,
            It.Is<GetOrganisationResponse>(o => o.Ukprn == ukprn),
            It.Is<PatchOrganisationModel>(p => p.ProviderType == sessionModel.ProviderType),
            cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Employer)]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndSupportingToOtherWithApprenticeshipsAndUnits_ThenRedirectsToProviderSummary(
        ProviderType providerTypeChangedTo,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationPatchService> patchServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitsUpdateViewModel viewModel,
        UpdateProviderTypeCourseTypesSessionModel sessionModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var courseTypesWithApprentices = new List<int> { (int)CourseType.Apprenticeship };
        sessionModel.CourseTypeIds = courseTypesWithApprentices;
        sessionModel.ProviderType = providerTypeChangedTo;

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        var apprenticeshipUnitsSelected = true;

        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };

        viewModel.ApprenticeshipUnitsSelectionId = apprenticeshipUnitsSelected;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn,
            It.Is<UpdateCourseTypesModel>(
                c => c.CourseTypeIds.Count == sessionModel.CourseTypeIds.Count
                && c.CourseTypeIds[0] == sessionModel.CourseTypeIds[0]
                && c.CourseTypeIds[1] == sessionModel.CourseTypeIds[1]
                )
            , cancellationToken), Times.Once);

        patchServiceMock.Verify(p => p.OrganisationPatched(ukprn,
            It.Is<GetOrganisationResponse>(o => o.Ukprn == ukprn),
            It.Is<PatchOrganisationModel>(p => p.ProviderType == sessionModel.ProviderType),
            cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Employer)]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndSupportingToOtherWithUnitsAndNoApprenticeships_ThenRedirectsToProviderSummary(
        ProviderType providerTypeChangedTo,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationPatchService> patchServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitsUpdateViewModel viewModel,
        UpdateProviderTypeCourseTypesSessionModel sessionModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var courseTypesWithoutApprentices = new List<int>();
        sessionModel.CourseTypeIds = courseTypesWithoutApprentices;
        sessionModel.ProviderType = providerTypeChangedTo;

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        var apprenticeshipUnitsSelected = true;

        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };

        viewModel.ApprenticeshipUnitsSelectionId = apprenticeshipUnitsSelected;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn,
            It.Is<UpdateCourseTypesModel>(
                c => c.CourseTypeIds.Count == sessionModel.CourseTypeIds.Count
                && c.CourseTypeIds[0] == sessionModel.CourseTypeIds[0]
                )
            , cancellationToken), Times.Once);

        patchServiceMock.Verify(p => p.OrganisationPatched(ukprn,
            It.Is<GetOrganisationResponse>(o => o.Ukprn == ukprn),
            It.Is<PatchOrganisationModel>(p => p.ProviderType == sessionModel.ProviderType),
            cancellationToken), Times.Once);
    }

}
