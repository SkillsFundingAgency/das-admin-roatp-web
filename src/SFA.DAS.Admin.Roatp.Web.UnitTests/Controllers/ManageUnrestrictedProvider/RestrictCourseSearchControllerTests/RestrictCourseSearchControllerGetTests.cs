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

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.RestrictCourseSearchControllerTests;

public class RestrictCourseSearchControllerGetTests
{
    private const int Ukprn = 10019900;
    internal const string UnrestrictedLarsCode = "105";
    internal const string UnrestrictedCourseTitle = "Alpha course";
    internal const int UnrestrictedCourseLevel = 6;
    private const string AllowedRestrictedLarsCode = "300";

    [Test, MoqAutoData]
    public async Task WhenGettingRestrictCourseSearch_ThenReturnsCoursesTheProviderIsAllowedToDeliver(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] IValidator<RestrictCourseSearchSubmitModel> validator,
        [Greedy] RestrictCourseSearchController controller)
    {
        SetupSearchableCourses(outerApiClientMock);

        var actual = await controller.Index(Ukprn, CancellationToken.None) as ViewResult;
        var model = actual?.Model as RestrictCourseSearchViewModel;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(RestrictCourseSearchController.ViewPath);
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
        [Greedy] RestrictCourseSearchController controller)
    {
        outerApiClientMock
            .Setup(c => c.GetNotRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetNotRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.NotFound),
                new GetNotRestrictedApprenticeshipsResponse(),
                new RefitSettings(),
                null));

        var actual = await controller.Index(Ukprn, CancellationToken.None);

        actual.Should().BeOfType<NotFoundResult>();
    }

    internal static void SetupSearchableCourses(Mock<IOuterApiClient> outerApiClientMock)
    {
        outerApiClientMock
            .Setup(c => c.GetNotRestrictedApprenticeships(Ukprn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GetNotRestrictedApprenticeshipsResponse>(
                new HttpResponseMessage(HttpStatusCode.OK),
                new GetNotRestrictedApprenticeshipsResponse
                {
                    Courses =
                    [
                        new RestrictedCourseModel
                        {
                            LarsCode = UnrestrictedLarsCode,
                            Title = UnrestrictedCourseTitle,
                            Level = UnrestrictedCourseLevel
                        },
                        new RestrictedCourseModel
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
}
