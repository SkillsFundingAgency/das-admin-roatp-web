using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
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

public class WhenCompletingUpdateProviderTypeJourney
{
    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Employer)]
    public async Task AndChangingFromSupporting_WithApprenticeshipsAndNoUnits_ThenRedirectsToProviderSummary(
        ProviderType providerTypeChangedTo,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationPatchService> patchServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        ApprenticeshipUnitsUpdateViewModel viewModel = new() { ApprenticeshipUnitsSelectionId = false, OffersApprenticeships = true };
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());
        sut.SetupAuthenticatedUser();

        UpdateProviderTypeCourseTypesSessionModel sessionModel = new()
        {
            CourseTypeIds = [(int)CourseType.Apprenticeship],
            ProviderType = providerTypeChangedTo
        };

        sessionServiceMock
            .Setup(s => s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        var apprenticeshipUnitsSelected = false;

        viewModel.ApprenticeshipUnitsSelectionId = apprenticeshipUnitsSelected;

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        // Action
        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn,
            It.Is<UpdateCourseTypesModel>(
                c => c.CourseTypeIds.Count == sessionModel.CourseTypeIds.Count
                && c.CourseTypeIds[0] == sessionModel.CourseTypeIds[0]
                )
            , cancellationToken), Times.Once);

        patchServiceMock.Verify(p => p.OrganisationPatched(getOrganisationResponse.Ukprn,
            It.Is<GetOrganisationResponse>(o => o.Ukprn == getOrganisationResponse.Ukprn),
            It.Is<PatchOrganisationModel>(p => p.ProviderType == sessionModel.ProviderType),
            cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Employer)]
    public async Task AndUpdatingSupportingToOther_WithApprenticeshipsAndUnits_ThenRedirectsToProviderSummary(
        ProviderType providerTypeChangedTo,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationPatchService> patchServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        ApprenticeshipUnitsUpdateViewModel viewModel = new() { ApprenticeshipUnitsSelectionId = true, OffersApprenticeships = false };
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());

        UpdateProviderTypeCourseTypesSessionModel sessionModel = new()
        {
            CourseTypeIds = [(int)CourseType.Apprenticeship],
            ProviderType = providerTypeChangedTo
        };

        sessionServiceMock
            .Setup(s => s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        outerApiClientMock
            .Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        // Action
        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn,
            It.Is<UpdateCourseTypesModel>(
                c => c.CourseTypeIds.Count == sessionModel.CourseTypeIds.Count
                && c.CourseTypeIds[0] == sessionModel.CourseTypeIds[0]
                && c.CourseTypeIds[1] == sessionModel.CourseTypeIds[1]
                )
            , cancellationToken), Times.Once);

        patchServiceMock.Verify(p => p.OrganisationPatched(getOrganisationResponse.Ukprn,
            It.Is<GetOrganisationResponse>(o => o.Ukprn == getOrganisationResponse.Ukprn),
            It.Is<PatchOrganisationModel>(p => p.ProviderType == sessionModel.ProviderType),
            cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Employer)]
    public async Task AndUpdatingFromSupportingToOther_WithUnitsAndNoApprenticeships_ThenRedirectsToProviderSummary(
        ProviderType providerTypeChangedTo,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IOrganisationPatchService> patchServiceMock,
        [Frozen] Mock<IValidator<ApprenticeshipUnitsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        ApprenticeshipUnitsUpdateViewModel viewModel = new() { ApprenticeshipUnitsSelectionId = true, OffersApprenticeships = false };
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel)).Returns(validationResult);

        UpdateProviderTypeCourseTypesSessionModel sessionModel = new() { ProviderType = providerTypeChangedTo };

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        sut.SetupAuthenticatedUser();

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        // Action
        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn,
            It.Is<UpdateCourseTypesModel>(
                c => c.CourseTypeIds.Count == sessionModel.CourseTypeIds.Count
                && c.CourseTypeIds[0] == sessionModel.CourseTypeIds[0]
                )
            , cancellationToken), Times.Once);

        patchServiceMock.Verify(p => p.OrganisationPatched(getOrganisationResponse.Ukprn,
            It.Is<GetOrganisationResponse>(o => o.Ukprn == getOrganisationResponse.Ukprn),
            It.Is<PatchOrganisationModel>(p => p.ProviderType == sessionModel.ProviderType),
            cancellationToken), Times.Once);
    }
}
