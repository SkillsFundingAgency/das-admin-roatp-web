using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

public class UnrestrictedCoursesControllerTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndQueryIsEmpty_ThenReturnsEmptyList(
        [Frozen] Mock<IUnrestrictedCoursesService> serviceMock,
        [Greedy] UnrestrictedCoursesController sut,
        CancellationToken cancellationToken)
    {
        var result = await sut.Index(" ", cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().BeEquivalentTo(Array.Empty<UnrestrictedCourseSearchItem>());
        serviceMock.Verify(x => x.GetUnrestrictedCourses(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndQueryMatchesTitle_ThenReturnsMatchingCourses(
        [Frozen] Mock<IUnrestrictedCoursesService> serviceMock,
        [Greedy] UnrestrictedCoursesController sut,
        CancellationToken cancellationToken)
    {
        var courses = new List<RestrictedCourseModel>
        {
            new() { LarsCode = "100", Title = "Software developer", Level = 4, LearningType = LearningType.Apprenticeship },
            new() { LarsCode = "200", Title = "Business administrator", Level = 3, LearningType = LearningType.Apprenticeship }
        };
        serviceMock.Setup(x => x.GetUnrestrictedCourses(cancellationToken)).ReturnsAsync(courses);

        var result = await sut.Index("soft", cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        var matched = result!.Value as IEnumerable<UnrestrictedCourseSearchItem>;
        matched.Should().ContainSingle();
        matched!.Single().LarsCode.Should().Be("100");
        matched.Single().DisplayTitle.Should().Be("Software developer (Level 4)");
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndQueryMatchesLarsCode_ThenReturnsMatchingCourses(
        [Frozen] Mock<IUnrestrictedCoursesService> serviceMock,
        [Greedy] UnrestrictedCoursesController sut,
        CancellationToken cancellationToken)
    {
        var courses = new List<RestrictedCourseModel>
        {
            new() { LarsCode = "12345", Title = "Software developer", Level = 4, LearningType = LearningType.Apprenticeship },
            new() { LarsCode = "99999", Title = "Business administrator", Level = 3, LearningType = LearningType.Apprenticeship }
        };
        serviceMock.Setup(x => x.GetUnrestrictedCourses(cancellationToken)).ReturnsAsync(courses);

        var result = await sut.Index("123", cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        var matched = result!.Value as IEnumerable<UnrestrictedCourseSearchItem>;
        matched.Should().ContainSingle();
        matched!.Single().LarsCode.Should().Be("12345");
    }
}
