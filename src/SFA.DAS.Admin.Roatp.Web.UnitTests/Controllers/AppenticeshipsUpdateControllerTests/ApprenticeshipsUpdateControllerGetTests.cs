using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AppenticeshipsUpdateControllerTests;

public class ApprenticeshipsUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipsUpdate_AndNoMatchingDetails_ThenRedirectsToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ApprenticeshipsUpdateController sut,
       int ukprn)
    {
        var cancellationToken = new CancellationToken();
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            var result = actual! as RedirectToRouteResult;
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.Home);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipsUpdate_AndNotInSession_ThenRedirectsToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            var result = actual! as RedirectToRouteResult;
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.Home);
        }
    }

    [Test]
    [MoqInlineAutoData(ProviderType.Main)]
    [MoqInlineAutoData(ProviderType.Supporting)]
    public async Task WhenGettingApprenticeshipsUpdate_AndNotEmployer_ThenRedirectsToHome(
        ProviderType providerType,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.ProviderType = providerType;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.RouteName.Should().Be(RouteNames.Home);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipsUpdate_AndEmployerDoesNotOfferApprenticeshipsOrUnits_ThenBuildsViewModelWithNoSelected(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = [];

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        var model = actual?.Model as OfferApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.IsApprenticeshipsOffered.Should().BeFalse();
            model.ApprenticeshipsSelection.Should().BeEquivalentTo(BuildApprenticeshipTypesChoices(false));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipsUpdate_AndEmployerDoesNotOfferApprenticeships_ThenBuildsViewModelWithNoSelected(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = [new() { CourseType = CourseType.ShortCourse }];

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        var model = actual?.Model as OfferApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.IsApprenticeshipsOffered.Should().BeFalse();
            model.ApprenticeshipsSelection.Should().BeEquivalentTo(BuildApprenticeshipTypesChoices(false));
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipsUpdate_AndEmployerOffersApprenticeships_ThenRedirectsToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = ukprn;
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = [new() { CourseType = CourseType.Apprenticeship }];

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.RouteName.Should().Be(RouteNames.ProviderSummary);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipsUpdate_AndSessionHasNoCourseTypes_ThenBuildsViewModelWithNoSelection(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(new UpdateProviderTypeCourseTypesSessionModel { CourseTypeIds = [] });

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        var model = actual?.Model as OfferApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.IsApprenticeshipsOffered.Should().BeNull();
            model.ApprenticeshipsSelection.Should().BeEquivalentTo(BuildApprenticeshipTypesChoices(null));
        }
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public async Task WhenGettingApprenticeshipsUpdate_AndMatchingDetailsAreInSession_ThenBuildsViewModelFromSession(
        bool containsApprenticeships,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn)
    {
        var cancellationToken = new CancellationToken();
        UpdateProviderTypeCourseTypesSessionModel sessionModel = new UpdateProviderTypeCourseTypesSessionModel
        {
            CourseTypeIds = new List<int>()
        };

        if (containsApprenticeships)
        {
            sessionModel.CourseTypeIds.Add((int)CourseType.Apprenticeship);
        }
        else
        {
            sessionModel.CourseTypeIds.Add((int)CourseType.ShortCourse);
        }

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        getOrganisationResponse.Ukprn = ukprn;

        var expectedApprenticeshipTypesChoices = BuildApprenticeshipTypesChoices(containsApprenticeships);

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        var model = actual?.Model as OfferApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.ApprenticeshipsSelection.Should().BeEquivalentTo(expectedApprenticeshipTypesChoices);
            model.IsApprenticeshipsOffered.Should().Be(containsApprenticeships);
        }
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipTypesChoices(bool? containsApprenticeships)
    {
        return
        [
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeships is true },
            new() { Description = "No", Id = false, IsSelected = containsApprenticeships is false }
        ];
    }
}
