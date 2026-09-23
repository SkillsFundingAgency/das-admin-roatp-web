using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ProviderRestrictedCourseSearchControllerTests;

public class ProviderRestrictedCourseSearchControllerGetTests
{
    private const int Ukprn = 10019900;
    private const string UnrestrictedLarsCode = "105";
    private const string UnrestrictedCourseTitle = "Alpha course";
    private const int UnrestrictedCourseLevel = 6;
    private const string AllowedRestrictedLarsCode = "300";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictCourseSearch_ThenReturnsCoursesTheProviderIsAllowedToDeliver(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] IValidator<ProviderRestrictedCourseSearchSubmitModel> validator,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupSearchableCourses(outerApiClientMock);

        var actual = await sut.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = actual?.Model as ProviderRestrictedCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(ProviderRestrictedCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.Courses.Select(course => course.Value).Should().Equal(
                UnrestrictedLarsCode,
                AllowedRestrictedLarsCode);
            model.Courses.Select(course => course.Text).Should().Equal(
                "Alpha course (Level 6)",
                "Beta course (Level 4)");
        }

        sessionServiceMock.Verify(s => s.Delete(SessionKeys.RestrictCourse), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictCourseSearch_AndNotRestrictedApprenticeshipsAreNotFound_ThenReturnsNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupNotRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.NotFound);

        var actual = await sut.Index(Ukprn, CancellationToken.None);

        actual.Should().BeOfType<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictCourseSearch_AndResponseContentIsNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupNotRestrictedApprenticeships(outerApiClientMock, HttpStatusCode.OK, null);

        var actual = await sut.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = actual?.Model as ProviderRestrictedCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(ProviderRestrictedCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.Courses.Should().BeEmpty();
        }
    }

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictCourseSearch_AndCoursesAreNull_ThenReturnsViewWithNoCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] ProviderRestrictedCourseSearchController sut)
    {
        SetupNotRestrictedApprenticeships(
            outerApiClientMock,
            HttpStatusCode.OK,
            new GetNotRestrictedApprenticeshipsResponse { Courses = null! });

        var actual = await sut.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = actual?.Model as ProviderRestrictedCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(ProviderRestrictedCourseSearchController.ViewPath);
            model.Should().NotBeNull();
            model!.Ukprn.Should().Be(Ukprn);
            model.Courses.Should().BeEmpty();
        }
    }

    private static void SetupSearchableCourses(Mock<IOuterApiClient> outerApiClientMock)
    {
        outerApiClientMock
            .Setup(c => c.GetNotRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetNotRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                new GetNotRestrictedApprenticeshipsResponse
                {
                    Courses =
                    [
                        new NotRestrictedApprenticeshipModel
                        {
                            LarsCode = UnrestrictedLarsCode,
                            Title = UnrestrictedCourseTitle,
                            Level = UnrestrictedCourseLevel
                        },
                        new NotRestrictedApprenticeshipModel
                        {
                            LarsCode = AllowedRestrictedLarsCode,
                            Title = "Beta course",
                            Level = 4
                        }
                    ]
                },
                new RefitSettings(),
                null));
    }

    private static void SetupNotRestrictedApprenticeships(
        Mock<IOuterApiClient> outerApiClientMock,
        HttpStatusCode statusCode,
        GetNotRestrictedApprenticeshipsResponse? response = null)
    {
        outerApiClientMock
            .Setup(c => c.GetNotRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetNotRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(statusCode),
                response,
                new RefitSettings(),
                null));
    }
}
