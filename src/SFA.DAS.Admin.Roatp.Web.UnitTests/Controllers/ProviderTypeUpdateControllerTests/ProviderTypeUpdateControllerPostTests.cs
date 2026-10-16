using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderTypeUpdateControllerTests;

public class ProviderTypeUpdateControllerPostTests
{
    [Test, MoqAutoData]
    public async Task WhenPostingProviderTypeUpdate_AndNoMatchingDetails_ThenRedirectsToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ProviderTypeUpdateController sut,
       ProviderTypeUpdateViewModel viewModel,
       int ukprn,
       CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingProviderTypeUpdate_AndNoProviderTypeChange_ThenRedirectsToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        viewModel.ProviderTypeId = (int)getOrganisationResponse.ProviderType;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));
        organisationPatchService.Setup(x => x.OrganisationPatched(getOrganisationResponse.Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))
            .ReturnsAsync(false);

        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual as RedirectToRouteResult;
        result.Should().NotBeNull();
        result.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main, ProviderType.Employer)]
    [MoqInlineAutoData(ProviderType.Employer, ProviderType.Main)]
    public async Task WhenPostingProviderTypeUpdate_AndSwappingMainAndEmployer_ThenRedirectsToProviderSummary(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel)
    {
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));
        organisationPatchService.Setup(x => x.OrganisationPatched(getOrganisationResponse.Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))
            .ReturnsAsync(true);

        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual as RedirectToRouteResult;
        result.Should().NotBeNull();
        result.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenPostingProviderTypeUpdate_AndSwappingFromEmployerToMain_ThenCreatesCourseTypeInRoatpCM(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        getOrganisationResponse.AllowedCourseTypes =
        [
            new() { CourseType = CourseType.Apprenticeship }
        ];
        CancellationToken cancellationToken = new();
        sut.SetupAuthenticatedUser();
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        ProviderTypeUpdateViewModel viewModel = new() { ProviderTypeId = (int)ProviderType.Main };
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));
        organisationPatchService.Setup(x => x.OrganisationPatched(getOrganisationResponse.Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))
            .ReturnsAsync(true);

        await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        outerApiClientMock.Verify(o => o.PutCourseTypes(getOrganisationResponse.Ukprn, It.Is<UpdateCourseTypesModel>(m => m.CourseTypeIds.Contains((int)CourseType.Apprenticeship) && m.CourseTypeIds.Count == 1), cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Supporting, ProviderType.Employer)]
    [MoqInlineAutoData(ProviderType.Supporting, ProviderType.Main)]
    public async Task WhenPostingProviderTypeUpdate_AndSupportingToMainOrEmployer_ThenRedirectsToApprenticeshipsUpdate(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Frozen] Mock<ISessionService> sessionMock,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse)
    {
        CancellationToken cancellationToken = new();
        getOrganisationResponse.ProviderType = providerType;
        ProviderTypeUpdateViewModel viewModel = new() { ProviderTypeId = (int)providerTypeChange };

        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));
        organisationPatchService.Setup(x => x.OrganisationPatched(getOrganisationResponse.Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken)).ReturnsAsync(true);

        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual as RedirectToRouteResult;
        result.Should().NotBeNull();
        result.RouteName.Should().Be(RouteNames.ApprenticeshipsUpdate);
        outerApiClientMock.Verify(o => o.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken), Times.Once);
        sessionMock.Verify(x => x.Set<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes, It.Is<UpdateProviderTypeCourseTypesSessionModel>(x => x.ProviderType == providerTypeChange)), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main, ProviderType.Supporting)]
    [MoqInlineAutoData(ProviderType.Employer, ProviderType.Supporting)]
    public async Task WhenPostingProviderTypeUpdate_AndChangingToSupportingWithNoNonStandardCourseTypes_ThenRedirectsToProviderSummary(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel)
    {
        CancellationToken cancellationToken = new();
        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { CourseType = CourseType.Apprenticeship }
        };
        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(getOrganisationResponse.Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))
            .ReturnsAsync(true);

        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);

        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main, ProviderType.Supporting)]
    [MoqInlineAutoData(ProviderType.Employer, ProviderType.Supporting)]
    public async Task WhenPostingProviderTypeUpdate_AndChangingToSupportingWithNonStandardCourseTypes_ThenRedirectsToProviderSummary(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel)
    {
        CancellationToken cancellationToken = new();
        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { CourseType = CourseType.Apprenticeship },
            new() { CourseType = CourseType.ShortCourse }
        };

        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(getOrganisationResponse.Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))
            .ReturnsAsync(true);

        var actual = await sut.Index(getOrganisationResponse.Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken), Times.Once);
    }
}
