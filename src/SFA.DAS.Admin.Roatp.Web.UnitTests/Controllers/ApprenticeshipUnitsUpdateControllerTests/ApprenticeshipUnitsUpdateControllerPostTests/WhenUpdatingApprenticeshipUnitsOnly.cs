using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ApprenticeshipUnitsUpdateControllerTests.ApprenticeshipUnitsUpdateControllerPostTests;

public class WhenUpdatingApprenticeshipUnitsOnly
{
    [Test, MoqAutoData]
    public async Task AndUkprnNotFound_ThenRedirectsToHome(
      [Frozen] Mock<IOuterApiClient> outerApiClientMock,
      [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validatorMock,
      [Greedy] ApprenticeshipUnitsUpdateController sut,
      ApprenticeshipUnitsUpdateViewModel viewModel,
      int ukprn)
    {
        CancellationToken cancellationToken = new();
        validatorMock.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());
        outerApiClientMock
            .Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        // Action
        var actual = await sut.Index(ukprn, viewModel, cancellationToken);

        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.Home);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.IsAny<UpdateCourseTypesModel>(), cancellationToken), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task AndNoChangeInApprenticeshipUnitsSelection_ThenRedirectsToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validatorMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        ApprenticeshipUnitsUpdateViewModel viewModel = new() { ApprenticeshipUnitsSelectionId = false, OffersApprenticeships = true };
        validatorMock.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());
        sessionServiceMock
            .Setup(s => s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.AllowedCourseTypes = [new() { CourseType = CourseType.Apprenticeship }];

        outerApiClientMock
            .Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        // Action
        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn, It.IsAny<UpdateCourseTypesModel>(), cancellationToken), Times.Never);
    }

    [Test]
    [MoqInlineAutoData(true, 2)]
    [MoqInlineAutoData(false, 1)]
    public async Task AndApprenticeshipUnitsAdded_ThenPushesApprenticeshipUnitCourseTypes(
       bool isStandardCourseTypePresent,
       int expectedPutCourseTypes,
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Frozen] Mock<ISessionService> sessionServiceMock,
       [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
       [Greedy] ApprenticeshipUnitsUpdateController sut,
       GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        ApprenticeshipUnitsUpdateViewModel viewModel = new() { ApprenticeshipUnitsSelectionId = true, OffersApprenticeships = isStandardCourseTypePresent };
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        var courseTypes = new List<AllowedCourseType>();
        if (isStandardCourseTypePresent) courseTypes.Add(new AllowedCourseType { CourseType = CourseType.Apprenticeship });
        getOrganisationResponse.AllowedCourseTypes = courseTypes;
        outerApiClientMock
            .Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        List<int> expectedCourseTypeIds = isStandardCourseTypePresent ? [1, 2] : [2];

        // Action
        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn, It.Is<UpdateCourseTypesModel>(c => c.CourseTypeIds.SequenceEqual(expectedCourseTypeIds)), cancellationToken), Times.Once);
        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test, MoqInlineAutoData]
    public async Task AndApprenticeshipUnitsRemoved_ThenPutsCourseTypes(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        ApprenticeshipUnitsUpdateViewModel viewModel = new() { ApprenticeshipUnitsSelectionId = false, OffersApprenticeships = true };
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());

        sessionServiceMock.Setup(s => s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes)).Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.AllowedCourseTypes =
        [
            new() { CourseType = CourseType.Apprenticeship },
            new() { CourseType = CourseType.ShortCourse }
        ];

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        List<int> expectedCourseTypeIds = [1];

        // Action
        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn, It.Is<UpdateCourseTypesModel>(c => c.CourseTypeIds.SequenceEqual(expectedCourseTypeIds)), cancellationToken), Times.Once);
        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test]
    [MoqInlineAutoData]
    public async Task WhenPostingApprenticeshipUnitsUpdate_AndValidationTriggered_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        ApprenticeshipUnitsUpdateViewModel viewModel,
        int ukprn)
    {
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        var validationResult = new ValidationResult([new ValidationFailure("Field", "Error")]);
        validator.Setup(x => x.Validate(viewModel)).Returns(validationResult);

        var selectionChoice = false;

        viewModel.ApprenticeshipUnitsSelectionId = selectionChoice;


        // Action
        var actual = await sut.Index(ukprn, viewModel, cancellationToken);

        actual.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<ApprenticeshipUnitsUpdateViewModel>();
        outerApiClientMock.Verify(o => o.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
