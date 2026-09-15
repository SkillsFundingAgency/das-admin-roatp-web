using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider;

[TestFixture]
public class RestrictedApprenticeshipsControllerGetTests
{
    private const string ProviderSummaryUrl = "/providers/10019900";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndProviderNameIsInTempData_ThenReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        GetRestrictedApprenticeshipsResponse response,
        string providerName,
        int ukprn)
    {
        response.Courses =
        [
            new RestrictedApprenticeshipModel
            {
                LarsCode = "200",
                Title = "Zebra course",
                Level = 3,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(-1),
                IsClosedToNewStarts = true
            },
            new RestrictedApprenticeshipModel
            {
                LarsCode = "105",
                Title = "Alpha course",
                Level = 6,
                LastDateStarts = DateTime.UtcNow.Date.AddDays(5),
                IsClosedToNewStarts = false
            }
        ];

        SetupTempData(sut);
        sut.TempData[RestrictedApprenticeshipsController.ProviderLegalNameTempDataKey] = providerName;
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, response);
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(RestrictedApprenticeshipsController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(providerName);
            model.BackLinkUrl.Should().Be(ProviderSummaryUrl);
            model.BackLinkText.Should().Be(RestrictedApprenticeshipsViewModel.BackLinkTextValue);
            model.RestrictACourseUrl.Should().Be(RestrictedCoursesUrl);
            model.HasCourses.Should().BeTrue();
            model.Courses.Select(course => course.LarsCode).Should().Equal("105", "200");
        }

        outerApiClientMock.Verify(c => c.GetOrganisation(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndProviderNameIsNotInTempData_ThenLoadsNameFromOrganisation(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        GetOrganisationResponse organisationResponse,
        GetRestrictedApprenticeshipsResponse restrictedResponse,
        int ukprn)
    {
        organisationResponse.Ukprn = ukprn;
        restrictedResponse.Courses = [];

        SetupTempData(sut);
        SetupOrganisation(outerApiClientMock, ukprn, organisationResponse, HttpStatusCode.OK);
        SetupRestrictedApprenticeships(outerApiClientMock, ukprn, restrictedResponse);
        SetupUrlHelper(sut);

        var result = await sut.Index(ukprn, CancellationToken.None) as ViewResult;
        var model = result?.Model as RestrictedApprenticeshipsViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be(organisationResponse.LegalName);
            model.HasNoCourses.Should().BeTrue();
            sut.TempData.Peek(RestrictedApprenticeshipsController.ProviderLegalNameTempDataKey)
                .Should().Be(organisationResponse.LegalName);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndOrganisationIsNotFound_ThenRedirectsToNotFoundPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        GetOrganisationResponse organisationResponse,
        int ukprn)
    {
        SetupTempData(sut);
        SetupOrganisation(outerApiClientMock, ukprn, organisationResponse, HttpStatusCode.NotFound);

        var result = await sut.Index(ukprn, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
        }

        outerApiClientMock.Verify(
            c => c.GetRestrictedApprenticeships(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictedApprenticeships_AndRestrictedApprenticeshipsAreNotFound_ThenRedirectsToNotFoundPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedApprenticeshipsController sut,
        string providerName,
        int ukprn)
    {
        SetupTempData(sut);
        sut.TempData[RestrictedApprenticeshipsController.ProviderLegalNameTempDataKey] = providerName;
        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound),
                new GetRestrictedApprenticeshipsResponse(),
                new RefitSettings(),
                null));

        var result = await sut.Index(ukprn, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundResult>();
        }
    }

    private static void SetupUrlHelper(RestrictedApprenticeshipsController sut)
    {
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderSummary, ProviderSummaryUrl)
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);
    }

    private static void SetupTempData(RestrictedApprenticeshipsController sut)
    {
        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        sut.TempData = new TempDataDictionary(sut.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
    }

    private static void SetupRestrictedApprenticeships(
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        GetRestrictedApprenticeshipsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedApprenticeships(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                response,
                new RefitSettings(),
                null));
    }

    private static void SetupOrganisation(
        Mock<IOuterApiClient> outerApiClientMock,
        int ukprn,
        GetOrganisationResponse response,
        HttpStatusCode statusCode)
    {
        outerApiClientMock
            .Setup(c => c.GetOrganisation(ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetOrganisationResponse>(
                new HttpResponseMessage(statusCode),
                response,
                new RefitSettings(),
                null));
    }
}
