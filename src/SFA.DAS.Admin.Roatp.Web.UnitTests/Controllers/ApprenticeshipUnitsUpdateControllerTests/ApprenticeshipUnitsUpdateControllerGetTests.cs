using System.Net;
using AutoFixture.NUnit4;
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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ApprenticeshipUnitsUpdateControllerTests;

public class ApprenticeshipUnitsUpdateControllerGetTests
{
    private const int Ukprn = 10019900;

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipUnitsUpdate_AndNoMatchingDetails_ThenRedirectsToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ApprenticeshipUnitsUpdateController sut,
       int ukprn)
    {
        var cancellationToken = new CancellationToken();
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.NotFound), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipUnitsUpdate_AndMatchingDetailsNotInSession_ThenBuildsViewModelFromOrganisationResponse(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        bool containsApprenticeshipUnits,
        bool containsApprenticeships)
    {
        var cancellationToken = new CancellationToken();
        sessionServiceMock.Setup(s =>
                    s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
                .Returns((UpdateProviderTypeCourseTypesSessionModel)null!);

        getOrganisationResponse.Ukprn = Ukprn;
        var allowedCourseTypes = new List<AllowedCourseType>();

        if (containsApprenticeshipUnits)
        {
            allowedCourseTypes.Add(new() { CourseType = CourseType.ShortCourse });
        }

        if (containsApprenticeships)
        {
            allowedCourseTypes.Add(new() { CourseType = CourseType.Apprenticeship });
        }

        getOrganisationResponse.AllowedCourseTypes = allowedCourseTypes;

        var expectedApprenticeshipTypesChoices = BuildApprenticeshipTypesChoices(containsApprenticeshipUnits);
        outerApiClientMock.Setup(x => x.GetOrganisation(Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(Ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ApprenticeshipUnitsUpdateViewModel;
        model.Should().NotBeNull();
        model.ApprenticeshipUnitsSelection.Should().BeEquivalentTo(expectedApprenticeshipTypesChoices);
        model.OffersApprenticeships.Should().Be(containsApprenticeships);
        model.ApprenticeshipUnitsSelectionId.Should().Be(containsApprenticeshipUnits);
    }


    [Test, MoqAutoData]
    public async Task WhenGettingApprenticeshipUnitsUpdate_AndMatchingDetailsAreInSession_ThenBuildsViewModelFromSession(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipUnitsUpdateController sut,
        GetOrganisationResponse getOrganisationResponse,
        bool containsApprenticeshipUnits,
        bool containsApprenticeships)
    {
        var cancellationToken = new CancellationToken();
        UpdateProviderTypeCourseTypesSessionModel sessionModel = new UpdateProviderTypeCourseTypesSessionModel
        {
            CourseTypeIds = new List<int>()
        };

        if (containsApprenticeships) { sessionModel.CourseTypeIds.Add((int)CourseType.Apprenticeship); }
        if (containsApprenticeshipUnits) { sessionModel.CourseTypeIds.Add((int)CourseType.ShortCourse); }

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        getOrganisationResponse.Ukprn = Ukprn;

        var expectedApprenticeshipTypesChoices =
                 new List<ApprenticeshipUnitsSelectionModel>
                {
                    new() { Description = "Yes", Id = true, IsSelected = false},
                    new() { Description = "No", Id = false, IsSelected = false},
                };

        outerApiClientMock.Setup(x => x.GetOrganisation(Ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), getOrganisationResponse, new RefitSettings(), null));

        var actual = await sut.Index(Ukprn, cancellationToken) as ViewResult;
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
