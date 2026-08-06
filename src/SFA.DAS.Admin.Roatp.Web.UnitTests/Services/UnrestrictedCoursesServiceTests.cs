using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Services;

public class UnrestrictedCoursesServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndCoursesAreInSession_ThenReturnsSessionCourses(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCoursesService sut,
        List<RestrictedCourseModel> courses,
        CancellationToken cancellationToken)
    {
        sessionServiceMock.Setup(x => x.Get<List<RestrictedCourseModel>>(SessionKeys.GetUnrestrictedCourses))
            .Returns(courses);

        var returnedCourses = await sut.GetUnrestrictedCourses(cancellationToken);

        returnedCourses.Should().BeEquivalentTo(courses);
        sessionServiceMock.Verify(x => x.Get<List<RestrictedCourseModel>>(SessionKeys.GetUnrestrictedCourses), Times.Once);
        clientMock.Verify(x => x.GetRestrictedCourses(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        sessionServiceMock.Verify(x => x.Set(SessionKeys.GetUnrestrictedCourses, It.IsAny<List<RestrictedCourseModel>>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndCoursesAreNotInSession_ThenFetchesStoresAndReturnsCourses(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] UnrestrictedCoursesService sut,
        List<RestrictedCourseModel> courses,
        CancellationToken cancellationToken)
    {
        sessionServiceMock.Setup(x => x.Get<List<RestrictedCourseModel>>(SessionKeys.GetUnrestrictedCourses))
            .Returns((List<RestrictedCourseModel>)null!);
        clientMock.Setup(x => x.GetRestrictedCourses(false, cancellationToken))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = courses });

        var returnedCourses = await sut.GetUnrestrictedCourses(cancellationToken);

        returnedCourses.Should().BeEquivalentTo(courses);
        sessionServiceMock.Verify(x => x.Get<List<RestrictedCourseModel>>(SessionKeys.GetUnrestrictedCourses), Times.Once);
        clientMock.Verify(x => x.GetRestrictedCourses(false, cancellationToken), Times.Once);
        sessionServiceMock.Verify(x => x.Set(SessionKeys.GetUnrestrictedCourses, courses), Times.Once);
    }
}
