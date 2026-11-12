using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AppenticeshipsUpdateControllerTests;

public class ApprenticeshipsUpdateControllerPostTests
{
    private const int Ukprn = 10019900;

    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndNoMatchingDetails_ThenRedirectsToHome(
     [Frozen] Mock<IOuterApiClient> outerApiClientMock,
     [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
     [Frozen] Mock<ISessionService> sessionServiceMock,
     [Greedy] ApprenticeshipsUpdateController sut,
     OfferApprenticeshipsSubmitModel submitModel,
     int ukprn)
    {
        validator.Setup(x => x.Validate(submitModel)).Returns(new ValidationResult());
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, submitModel, new CancellationToken());

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            var result = actual! as RedirectToRouteResult;
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.Home);
        }
    }

    [Test]
    [MoqInlineAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndValidationTriggered_ThenResetsWithCorrectViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Greedy] ApprenticeshipsUpdateController sut,
        OfferApprenticeshipsSubmitModel submitModel,
        int ukprn)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));
        validator.Setup(x => x.Validate(submitModel))
            .Returns(validationResult);

        var selectedNoId = false;
        submitModel.IsApprenticeshipsOffered = selectedNoId;

        var expectedSelections = BuildApprenticeshipsChoices(selectedNoId);

        var actual = await sut.Index(ukprn, submitModel, new CancellationToken());
        var result = actual as ViewResult;
        var model = result?.Model as OfferApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            result.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.IsApprenticeshipsOffered.Should().Be(selectedNoId);
            model.ApprenticeshipsSelection.Should().BeEquivalentTo(expectedSelections);
            outerApiClientMock.Verify(o => o.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    [MoqInlineAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndNoSelected_ThenMovesToApprenticeshipUnitsUpdate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        validator.Setup(x => x.Validate(submitModel))
            .Returns(new ValidationResult());

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(new UpdateProviderTypeCourseTypesSessionModel { CourseTypeIds = [] });

        submitModel.IsApprenticeshipsOffered = false;

        var actual = await sut.Index(Ukprn, submitModel, new CancellationToken());
        var result = actual as RedirectToRouteResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ApprenticeshipUnitsUpdate);
            outerApiClientMock.Verify(o => o.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        sessionServiceMock.Verify(x => x.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys
            .UpdateSupportingProviderCourseTypes), Times.Once);
        sessionServiceMock.Verify(x => x.Set(SessionKeys
            .UpdateSupportingProviderCourseTypes, It.IsAny<UpdateProviderTypeCourseTypesSessionModel>()), Times.Never);
    }

    [Test]
    [MoqInlineAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndYesSelected_ThenSetsSessionAndMovesToApprenticeshipUnitsUpdate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        validator.Setup(x => x.Validate(submitModel))
            .Returns(new ValidationResult());

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(new UpdateProviderTypeCourseTypesSessionModel { CourseTypeIds = [] });

        submitModel.IsApprenticeshipsOffered = true;

        var actual = await sut.Index(Ukprn, submitModel, new CancellationToken());
        var result = actual as RedirectToRouteResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ApprenticeshipUnitsUpdate);
            outerApiClientMock.Verify(o => o.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        sessionServiceMock.Verify(x => x.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys
            .UpdateSupportingProviderCourseTypes), Times.Once);
        sessionServiceMock.Verify(x => x.Set(SessionKeys
            .UpdateSupportingProviderCourseTypes, It.Is<UpdateProviderTypeCourseTypesSessionModel>(
            c => c.CourseTypeIds.Count == 1
            && c.CourseTypeIds[0] == (int)CourseType.Apprenticeship
            )), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public async Task WhenPostingApprenticeshipsUpdate_AndEmployerSelectsYes_ThenPutsApprenticeshipCourseTypeAndRedirectsToProviderSummary(
        bool hasShortCourse,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        var cancellationToken = new CancellationToken();
        validator.Setup(x => x.Validate(submitModel)).Returns(new ValidationResult());
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        sut.SetupAuthenticatedUser();
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = hasShortCourse
            ? [new() { CourseType = CourseType.ShortCourse }]
            : [];
        submitModel.IsApprenticeshipsOffered = true;

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        List<int> expectedCourseTypeIds = hasShortCourse ? [1, 2] : [1];

        var actual = await sut.Index(getOrganisationResponse.Ukprn, submitModel, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
            outerApiClientMock.Verify(o => o.PutCourseTypes(
                getOrganisationResponse.Ukprn,
                It.Is<UpdateCourseTypesModel>(c => c.CourseTypeIds.SequenceEqual(expectedCourseTypeIds)),
                cancellationToken), Times.Once);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndEmployerSelectsNo_ThenRedirectsToProviderSummaryWithoutUpdatingCourseTypes(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        var cancellationToken = new CancellationToken();
        validator.Setup(x => x.Validate(submitModel)).Returns(new ValidationResult());
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        sut.SetupAuthenticatedUser();
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = [new() { CourseType = CourseType.ShortCourse }];
        submitModel.IsApprenticeshipsOffered = false;

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(getOrganisationResponse.Ukprn, submitModel, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
            outerApiClientMock.Verify(o => o.PutCourseTypes(
                It.IsAny<int>(),
                It.IsAny<UpdateCourseTypesModel>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndNotEmployer_ThenRedirectsToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        var cancellationToken = new CancellationToken();
        validator.Setup(x => x.Validate(submitModel)).Returns(new ValidationResult());
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.ProviderType = ProviderType.Main;
        submitModel.IsApprenticeshipsOffered = true;

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(getOrganisationResponse.Ukprn, submitModel, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.Home);
            outerApiClientMock.Verify(o => o.PutCourseTypes(
                It.IsAny<int>(),
                It.IsAny<UpdateCourseTypesModel>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndEmployerAlreadyOffersApprenticeshipsAndYesSelected_ThenRedirectsToProviderSummaryWithoutUpdatingCourseTypes(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        var cancellationToken = new CancellationToken();
        validator.Setup(x => x.Validate(submitModel)).Returns(new ValidationResult());
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        sut.SetupAuthenticatedUser();
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = [new() { CourseType = CourseType.Apprenticeship }];
        submitModel.IsApprenticeshipsOffered = true;

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(getOrganisationResponse.Ukprn, submitModel, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
            outerApiClientMock.Verify(o => o.PutCourseTypes(
                It.IsAny<int>(),
                It.IsAny<UpdateCourseTypesModel>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingApprenticeshipsUpdate_AndEmployerAlreadyOffersApprenticeships_ThenRedirectsToProviderSummaryWithoutUpdatingCourseTypes(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<OfferApprenticeshipsSubmitModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        OfferApprenticeshipsSubmitModel submitModel)
    {
        var cancellationToken = new CancellationToken();
        validator.Setup(x => x.Validate(submitModel)).Returns(new ValidationResult());
        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        sut.SetupAuthenticatedUser();
        getOrganisationResponse.ProviderType = ProviderType.Employer;
        getOrganisationResponse.AllowedCourseTypes = [new() { CourseType = CourseType.Apprenticeship }];
        submitModel.IsApprenticeshipsOffered = false;

        outerApiClientMock.Setup(x => x.GetOrganisation(getOrganisationResponse.Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(getOrganisationResponse.Ukprn, submitModel, cancellationToken);

        using (new AssertionScope())
        {
            actual.Should().BeOfType<RedirectToRouteResult>().Which.RouteName.Should().Be(RouteNames.ProviderSummary);
            outerApiClientMock.Verify(o => o.PutCourseTypes(
                It.IsAny<int>(),
                It.IsAny<UpdateCourseTypesModel>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeshipUnits)
    {
        return
        [
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits is true },
            new() { Description = "No", Id = false, IsSelected = containsApprenticeshipUnits is false }
        ];
    }
}
