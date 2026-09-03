using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.ChangeCourseRestrictionControllerTests;

[TestFixture]
public class ChangeCourseRestrictionControllerGetTests
{
    private const int Ukprn = 10007938;
    private const string LarsCode = "105";
    private const string RestrictedCourseDetailsUrl = $"/restricted-courses/{LarsCode}";
    private static readonly DateTime LastDateStarts = new(2027, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public async Task WhenProviderHasLastDateStarts_ThenReturnsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        SetupCourseWithLastDateStarts(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None) as ViewResult;

        var model = result!.Model as ChangeCourseRestrictionViewModel;
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeCourseRestrictionController.ViewPath);
            model.Should().NotBeNull();
            model!.ProviderName.Should().Be("BP TRAINING");
            model.Ukprn.Should().Be(Ukprn);
            model.CourseDisplayTitle.Should().Be("Academic professional (Level 7)");
            model.LastDateStarts.Should().Be(LastDateStarts);
            model.LastDateStartsText.Should().Be("01 Jun 2027");
            model.CancelUrl.Should().Be(RestrictedCourseDetailsUrl);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenProviderHasNoLastDateStarts_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = null
            }
        ];
        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenProviderDoesNotExistOnCourse_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeCourseRestrictionController sut,
        GetRestrictedCourseDetailsResponse response)
    {
        response.Providers = [];
        SetupCourseResponse(outerApiClientMock, response);

        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.RestrictedCourseDetails, RestrictedCourseDetailsUrl);

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task WhenCourseApiReturnsNotFound_ThenReturnsNotFound()
    {
        var outerApiClientMock = new Mock<IOuterApiClient>();
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound), null, new RefitSettings(), null));

        var sut = new ChangeCourseRestrictionController(
            outerApiClientMock.Object,
            Mock.Of<IValidator<ChangeCourseRestrictionSubmitModel>>());

        var result = await sut.Index(LarsCode, Ukprn, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenCourseApiReturnsUnexpectedError_ThenThrows(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ChangeCourseRestrictionController sut)
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            httpResponse,
            new RefitSettings());
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                httpResponse, null, new RefitSettings(), apiException));

        var act = () => sut.Index(LarsCode, Ukprn, CancellationToken.None);

        await act.Should().ThrowAsync<ApiException>();
    }

    [Test]
    public void ChangeCourseRestrictionOptions_ExposeExpectedValues()
    {
        using (new AssertionScope())
        {
            ChangeCourseRestrictionOptions.Change.Should().Be("Change");
            ChangeCourseRestrictionOptions.Remove.Should().Be("Remove");
        }
    }

    private static void SetupCourseWithLastDateStarts(
        Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse response)
    {
        response.LarsCode = LarsCode;
        response.CourseName = "Academic professional";
        response.Level = 7;
        response.Providers =
        [
            new ProviderCourseModel
            {
                Ukprn = Ukprn,
                ProviderName = "BP TRAINING",
                LastDateStarts = LastDateStarts
            }
        ];

        SetupCourseResponse(outerApiClientMock, response);
    }

    private static void SetupCourseResponse(
        Mock<IOuterApiClient> outerApiClientMock,
        GetRestrictedCourseDetailsResponse response)
    {
        outerApiClientMock
            .Setup(c => c.GetAllowedProvidersForCourse(LarsCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetRestrictedCourseDetailsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK), response, new RefitSettings(), null));
    }
}
