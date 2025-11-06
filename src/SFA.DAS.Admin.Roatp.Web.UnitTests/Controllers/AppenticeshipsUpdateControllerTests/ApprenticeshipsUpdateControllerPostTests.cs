using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
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
    [Test, MoqAutoData]
    public async Task Post_NoMatchingDetails_RedirectToHome(
     [Frozen] Mock<IOuterApiClient> outerApiClientMock,
     [Greedy] ApprenticeshipsUpdateController sut,
     ApprenticeshipsUpdateViewModel viewModel,
     int ukprn,
     CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((GetOrganisationResponse)null!);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test]
    [MoqInlineAutoData]
    public async Task Post_ValidationTriggered_ResetWithCorrectViewModelSetup(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ApprenticeshipsUpdateViewModel>> validator,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipsUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var selectedNoId = false;
        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };
        var courseTypes = new List<AllowedCourseType> { new() { CourseTypeId = 2, CourseTypeName = "Unit", LearningType = LearningType.ShortCourse } };

        getOrganisationResponse.AllowedCourseTypes = courseTypes;
        var currentCourseTypeIds = getOrganisationResponse.AllowedCourseTypes
            .Select(a => a.CourseTypeId).ToList();

        viewModel.ApprenticeshipsSelectionChoice = selectedNoId;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);

        var expectedSelections = BuildApprenticeshipsChoices(selectedNoId);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as ViewResult;
        result.Should().NotBeNull();
        var model = result!.Model as ApprenticeshipsUpdateViewModel;
        model.Should().NotBeNull();
        model.ApprenticeshipsSelectionChoice.Should().Be(selectedNoId);
        model.ApprenticeshipsSelection.Should().BeEquivalentTo(expectedSelections);
    }

    [Test]
    [MoqInlineAutoData]
    public async Task Post_NoSelected_MoveToApprenticeshipUnitsUpdate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ApprenticeshipsUpdateViewModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipsUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var selectedNoId = false;
        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };
        var courseTypes = new List<AllowedCourseType> { new() { CourseTypeId = 2, CourseTypeName = "Unit", LearningType = LearningType.ShortCourse } };

        getOrganisationResponse.AllowedCourseTypes = courseTypes;

        viewModel.ApprenticeshipsSelectionChoice = selectedNoId;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ApprenticeshipUnitsUpdate);
        sessionServiceMock.Verify(x => x.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys
            .UpdateSupportingProviderCourseTypes), Times.Never);
        sessionServiceMock.Verify(x => x.Set(SessionKeys
            .UpdateSupportingProviderCourseTypes, It.IsAny<UpdateProviderTypeCourseTypesSessionModel>()), Times.Never);
    }

    [Test]
    [MoqInlineAutoData]
    public async Task Post_YesSelected_SetInSession_MoveToApprenticeshipUnitsUpdate(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<ApprenticeshipsUpdateViewModel>> validator,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipsUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        validator.Setup(x => x.Validate(viewModel))
            .Returns(validationResult);

        var selectedNoId = true;
        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };
        var courseTypes = new List<AllowedCourseType> { new() { CourseTypeId = 2, CourseTypeName = "Unit", LearningType = LearningType.ShortCourse } };

        getOrganisationResponse.AllowedCourseTypes = courseTypes;
        viewModel.ApprenticeshipsSelectionChoice = selectedNoId;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ApprenticeshipUnitsUpdate);
        sessionServiceMock.Verify(x => x.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys
            .UpdateSupportingProviderCourseTypes), Times.Once);
        sessionServiceMock.Verify(x => x.Set(SessionKeys
            .UpdateSupportingProviderCourseTypes, It.Is<UpdateProviderTypeCourseTypesSessionModel>(
            c => c.CourseTypeIds.Count == 1
            && c.CourseTypeIds[0] == CourseTypes.ApprenticeshipId
            )), Times.Once);
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeshipUnits)
    {
        return new List<ApprenticeshipsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits is true},
            new() { Description = "No", Id = false, IsSelected =  containsApprenticeshipUnits is false}
        };
    }
}
