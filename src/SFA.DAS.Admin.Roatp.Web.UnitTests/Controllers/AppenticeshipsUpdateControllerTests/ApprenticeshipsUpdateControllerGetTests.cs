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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.AppenticeshipsUpdateControllerTests;
public class ApprenticeshipsUpdateControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_NoMatchingDetails_RedirectToHome(
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Greedy] ApprenticeshipsUpdateController sut,
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
    public async Task Get_NotInSession_RedirectToHome(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
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
        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken);
        actual.Should().NotBeNull();
        var result = actual! as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.Home);
    }


    [Test, MoqAutoData]
    public async Task Get_MatchingDetails_InSession_BuildViewModelFromSession(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        bool containsApprenticeships,
        CancellationToken cancellationToken)
    {
        UpdateProviderTypeCourseTypesSessionModel sessionModel = new UpdateProviderTypeCourseTypesSessionModel
        {
            CourseTypeIds = new List<int>()
        };

        if (containsApprenticeships) { sessionModel.CourseTypeIds.Add(CourseTypes.ApprenticeshipId); }

        sessionServiceMock.Setup(s =>
                s.Get<UpdateProviderTypeCourseTypesSessionModel>(SessionKeys.UpdateSupportingProviderCourseTypes))
            .Returns(sessionModel);

        getOrganisationResponse.Ukprn = ukprn;

        var expectedApprenticeshipTypesChoices = BuildApprenticeshipTypesChoices(containsApprenticeships);

        outerApiClientMock.Setup(x => x.GetOrganisation(ukprn, cancellationToken))!
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(new HttpResponseMessage(HttpStatusCode.OK), new GetOrganisationResponse(), new RefitSettings(), null));

        var actual = await sut.Index(ukprn, cancellationToken) as ViewResult;
        actual.Should().NotBeNull();
        var model = actual.Model as ApprenticeshipsUpdateViewModel;
        model.Should().NotBeNull();
        model.ApprenticeshipsSelection.Should().BeEquivalentTo(expectedApprenticeshipTypesChoices);
        model.ApprenticeshipsSelectionChoice.Should().Be(containsApprenticeships);
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipTypesChoices(bool containsApprenticeships)
    {
        return new List<ApprenticeshipsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeships},
            new() { Description = "No", Id = false, IsSelected = !containsApprenticeships},
        };
    }
}
