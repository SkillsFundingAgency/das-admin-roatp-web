using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ApprenticeshipUnitsUpdateControllerTests;
public class ApprenticeshipUnitsUpdateControllerPostTests
{
    [Test, MoqAutoData]
    public async Task Post_NoMatchingDetails_RedirectToHome(
      [Frozen] Mock<IOuterApiClient> outerApiClientMock,
      [Greedy] ApprenticeshipUnitsUpdateController sut,
      ApprenticeshipUnitUpdateViewModel viewModel,
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
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.IsAny<UpdateCourseTypesModel>(), cancellationToken), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Post_NoProviderTypeChange_RedirectToProviderSummary(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var courseTypes = new List<AllowedCourseType>
        {
            new() { CourseTypeId = 1, CourseTypeName = "Apprenticeship", LearningType = LearningType.Standard }
        };

        getOrganisationResponse.AllowedCourseTypes = courseTypes;

        viewModel.ApprenticeshipUnitSelectionChoice =
            getOrganisationResponse.AllowedCourseTypes.Any(a => a.CourseTypeId == CourseTypes.ApprenticeshipUnitId);

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.IsAny<UpdateCourseTypesModel>(), cancellationToken), Times.Never);
    }

    [Test]
    [MoqInlineAutoData(true, true, 1)]
    [MoqInlineAutoData(true, false, 2)]
    [MoqInlineAutoData(false, true, 0)]
    [MoqInlineAutoData(false, false, 1)]
    public async Task Post_ProviderTypeChange_ChangePosted(
       bool isStandardCourseTypePresent,
       bool isShortCourseTypePresent,
       int expectedPutCourseTypes,
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        ApprenticeshipUnitUpdateViewModel viewModel,
        int ukprn,
        CancellationToken cancellationToken)
    {
        sut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = MockedUser.Setup() }
        };
        var courseTypes = new List<AllowedCourseType>();
        if (isStandardCourseTypePresent) courseTypes.Add(new AllowedCourseType { CourseTypeId = 1, CourseTypeName = "Apprenticeship", LearningType = LearningType.Standard });
        if (isShortCourseTypePresent) courseTypes.Add(new AllowedCourseType { CourseTypeId = 2, CourseTypeName = "Unit", LearningType = LearningType.ShortCourse });

        getOrganisationResponse.AllowedCourseTypes = courseTypes;
        var currentCourseTypeIds = getOrganisationResponse.AllowedCourseTypes
            .Select(a => a.CourseTypeId).ToList();

        viewModel.ApprenticeshipUnitSelectionChoice = !isShortCourseTypePresent;

        getOrganisationResponse.Ukprn = ukprn;
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn.ToString(), cancellationToken))!
            .ReturnsAsync(getOrganisationResponse);


        var expectedCourseTypeIds = currentCourseTypeIds;
        if (isShortCourseTypePresent)
        {
            expectedCourseTypeIds = currentCourseTypeIds.Where(a => a != CourseTypes.ApprenticeshipUnitId).ToList();
        }
        else
        {
            expectedCourseTypeIds.Add(CourseTypes.ApprenticeshipUnitId);
        }

        var actual = await sut.Index(ukprn, viewModel, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.ProviderSummary);
        expectedCourseTypeIds.Count.Should().Be(expectedPutCourseTypes);

        switch (expectedPutCourseTypes)
        {
            case 0:
                outerApiClientMock.Verify(o => o.PutCourseTypes(ukprn, It.Is<UpdateCourseTypesModel>(
                    c => c.CourseTypeIds.Count == 0
                ), cancellationToken), Times.Once);
                break;
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
}
