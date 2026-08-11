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
    public async Task WhenGettingUnrestrictedCourses_ThenFetchesAndReturnsCourses(
        [Frozen] Mock<IOuterApiClient> clientMock,
        [Greedy] UnrestrictedCoursesService sut,
        List<RestrictedCourseModel> courses,
        CancellationToken cancellationToken)
    {
        clientMock.Setup(x => x.GetRestrictedCourses(false, cancellationToken))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = courses });

        var returnedCourses = await sut.GetUnrestrictedCourses(cancellationToken);

        returnedCourses.Should().BeEquivalentTo(courses);
        clientMock.Verify(x => x.GetRestrictedCourses(false, cancellationToken), Times.Once);
    }
}
