using AutoFixture.NUnit3;
using FluentAssertions;
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
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ApprenticeshipUnitsUpdateControllerTests;
public class ApprenticeshipUnitsUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ApprenticeshipUnitsUpdateController sut,
       int ukprn,
       CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_NotInSession_BuildViewModelFromGetOrganisationResponse(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        bool containsApprenticeshipUnits,
        bool containsApprenticeships,
        CancellationToken cancellationToken)
    {
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = ukprn;
        var allowedCourseTypes = new List<AllowedCourseType>();

        if (containsApprenticeshipUnits)
        {
            allowedCourseTypes.Add(new() { CourseTypeId = CourseTypes.ApprenticeshipUnitId });
        }

        if (containsApprenticeships)
        {
            allowedCourseTypes.Add(new() { CourseTypeId = CourseTypes.ApprenticeshipId });
        }

        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;

        var expectedApprenticeshipTypesChoices = BuildApprenticeshipTypesChoices(containsApprenticeshipUnits);
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ApprenticeshipUnitsUpdateViewModel;
        model.Should().NotBeNull();
        model.ApprenticeshipUnitsSelection.Should().BeEquivalentTo(expectedApprenticeshipTypesChoices);
        model.OffersApprenticeships.Should().Be(containsApprenticeships);
        model.ApprenticeshipUnitsSelectionId.Should().Be(containsApprenticeshipUnits);
    }


    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_InSession_BuildViewModelFromSession(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        bool containsApprenticeshipUnits,
        bool containsApprenticeships,
        CancellationToken cancellationToken)
    {
        UpdateProviderTypeCourseTypesSessionModel sessionModel = new UpdateProviderTypeCourseTypesSessionModel
        {
            CourseTypeIds = new List<int>()
        };

        if (containsApprenticeships) { sessionModel.CourseTypeIds.Add(CourseTypes.ApprenticeshipId); }
        if (containsApprenticeshipUnits) { sessionModel.CourseTypeIds.Add(CourseTypes.ApprenticeshipUnitId); }

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        getOrganisationResponse.Ukprn = ukprn;

        var expectedApprenticeshipTypesChoices =
                 new List<ApprenticeshipUnitsSelectionModel>
                {
                    new() { Description = "Yes", Id = true, IsSelected = false},
                    new() { Description = "No", Id = false, IsSelected = false},
                };

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ApprenticeshipUnitsUpdateViewModel;
        model.Should().NotBeNull();
        model.ApprenticeshipUnitsSelection.Should().BeEquivalentTo(expectedApprenticeshipTypesChoices);
        model.OffersApprenticeships.Should().Be(containsApprenticeships);
        model.ApprenticeshipUnitsSelectionId.Should().Be(null);

    }

    private static List<ApprenticeshipUnitsSelectionModel> BuildApprenticeshipTypesChoices(bool containsApprenticeshipUnits)
    {
        return new List<ApprenticeshipUnitsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits},
            new() { Description = "No", Id = false, IsSelected = !containsApprenticeshipUnits},
        };
    }
}
