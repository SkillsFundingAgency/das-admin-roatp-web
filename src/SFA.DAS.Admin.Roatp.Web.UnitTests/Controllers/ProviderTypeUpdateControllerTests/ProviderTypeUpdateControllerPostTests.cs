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
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ProviderTypeUpdateControllerTests;

public class ProviderTypeUpdateControllerPostTests
{
    private const int Ukprn = 10019900;

    [Test, MoqAutoData]
    public async Task WhenPostingProviderTypeUpdate_AndNoMatchingDetails_ThenRedirectsToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ProviderTypeUpdateController sut,
       ProviderTypeUpdateViewModel viewModel,
       int ukprn)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, viewModel, new CancellationToken());
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
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
        ProviderType providerType)
    {
        var cancellationToken = new CancellationToken();
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)getOrganisationResponse.ProviderType;
        getOrganisationResponse.Ukprn = Ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(false);

        var actual = await sut.Index(Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
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
        var cancellationToken = new CancellationToken();
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        getOrganisationResponse.Ukprn = Ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(Ukprn, cancellationToken), Times.Once);
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
        var cancellationToken = new CancellationToken();
        var viewModel = new ProviderTypeUpdateViewModel { ProviderTypeId = (int)providerTypeChange };
        getOrganisationResponse.ProviderType = providerType;
        getOrganisationResponse.Ukprn = Ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ApprenticeshipsUpdate);
        outerApiClientMock.Verify(o => o.GetOrganisation(Ukprn, cancellationToken), Times.Once);
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
        var cancellationToken = new CancellationToken();
        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { CourseType = CourseType.Apprenticeship }
        };
        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        getOrganisationResponse.Ukprn = Ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(Ukprn, cancellationToken), Times.Once);
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
        var cancellationToken = new CancellationToken();
        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { CourseType = CourseType.Apprenticeship },
            new() { CourseType = CourseType.ShortCourse }
        };

        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        getOrganisationResponse.Ukprn = Ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        organisationPatchService.Setup(x => x.OrganisationPatched(Ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(Ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(Ukprn, cancellationToken), Times.Once);
    }
}
