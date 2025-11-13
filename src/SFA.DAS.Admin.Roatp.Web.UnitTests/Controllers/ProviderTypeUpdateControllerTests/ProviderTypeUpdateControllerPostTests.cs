using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ProviderTypeUpdateController sut,
       ProviderTypeUpdateViewModel viewModel,
       int ukprn,
       CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((GetOrganisationResponse)null!);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_NoProviderTypeChange_RedirectToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel,
        ProviderType providerType,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)getOrganisationResponse.ProviderType;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(false);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main, ProviderType.Employer)]
    [MoqInlineAutoData(ProviderType.Employer, ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Supporting, ProviderType.Employer)]
    [MoqInlineAutoData(ProviderType.Supporting, ProviderType.Main)]
    public async Task Get_ProviderTypeChange_NotToSupporting_RedirectToProviderSummary(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(ukprn.ToString(), cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main, ProviderType.Supporting)]
    [MoqInlineAutoData(ProviderType.Employer, ProviderType.Supporting)]
    public async Task Get_ProviderTypeChange_ToSupporting_NonStandardCourseTypesNotPresent_RedirectToProviderSummary(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { CourseTypeId = 1, CourseTypeName = "Apprenticeship", LearningType = LearningType.Standard }
        };
        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(ukprn.ToString(), cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main, ProviderType.Supporting)]
    [MoqInlineAutoData(ProviderType.Employer, ProviderType.Supporting)]
    public async Task Get_ProviderTypeChange_ToSupporting_NonStandardCourseTypesPresent_RedirectToProviderSummary(
        ProviderType providerType,
        ProviderType providerTypeChange,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IOrganisationPatchService> organisationPatchService,
        [Greedy] ProviderTypeUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ProviderTypeUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var allowedCourseTypes = new List<AllowedCourseType>
        {
            new() { CourseTypeId = 1, CourseTypeName = "Apprenticeship", LearningType = LearningType.Standard },
            new() { CourseTypeId = 2, CourseTypeName = "Unit", LearningType = LearningType.ShortCourse}
        };

        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;
        getOrganisationResponse.ProviderType = providerType;
        viewModel.ProviderTypeId = (int)providerTypeChange;
        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<string>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(getOrganisationResponse);

        organisationPatchService.Setup(x => x.OrganisationPatched(ukprn, getOrganisationResponse, It.IsAny<PatchOrganisationModel>(), cancellationToken))!
            .ReturnsAsync(true);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.GetOrganisation(ukprn.ToString(), cancellationToken), Times.Once);
    }
}
