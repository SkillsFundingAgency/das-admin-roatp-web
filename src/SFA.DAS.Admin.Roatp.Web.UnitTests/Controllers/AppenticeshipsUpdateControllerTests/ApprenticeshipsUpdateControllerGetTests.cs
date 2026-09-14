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
    public async Task WhenGettingApprenticeshipsUpdate_AndNotInSession_ThenRedirectsToHome(
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


    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public async Task WhenGettingApprenticeshipsUpdate_AndMatchingDetailsAreInSession_ThenBuildsViewModelFromSession(
        bool containsApprenticeships,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ApprenticeshipsUpdateController sut,
        string selectOrganisationLink,
        GetOrganisationResponse getOrganisationResponse,
        int ukprn,
        CancellationToken cancellationToken)
    {
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

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipTypesChoices(bool containsApprenticeships)
    {
        return new List<ApprenticeshipsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = containsApprenticeships},
            new() { Description = "No", Id = false, IsSelected = !containsApprenticeships},
        };
    }
}
