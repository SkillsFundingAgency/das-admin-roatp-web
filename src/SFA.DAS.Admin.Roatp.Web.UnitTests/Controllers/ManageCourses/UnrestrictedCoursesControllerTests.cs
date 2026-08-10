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
    public async Task WhenGettingUnrestrictedCourses_AndSearchTermIsEmpty_ThenReturnsEmptyList(
        [Frozen] Mock<IUnrestrictedCoursesService> serviceMock,
        [Greedy] UnrestrictedCoursesController sut,
        CancellationToken cancellationToken)
    {
        const string searchTerm = " ";
        var result = await sut.Index(searchTerm, cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().BeEquivalentTo(Array.Empty<UnrestrictedCourseSearchItem>());
        serviceMock.Verify(x => x.GetUnrestrictedCourses(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndSearchTermIsNull_ThenReturnsEmptyList(
        [Frozen] Mock<IUnrestrictedCoursesService> serviceMock,
        [Greedy] UnrestrictedCoursesController sut,
        CancellationToken cancellationToken)
    {
        var result = await sut.Index(null!, cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().BeEquivalentTo(Array.Empty<UnrestrictedCourseSearchItem>());
        serviceMock.Verify(x => x.GetUnrestrictedCourses(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndMultipleCoursesMatch_ThenReturnsOrderedByTitleThenLevel(
        [Frozen] Mock<IUnrestrictedCoursesService> serviceMock,
        [Greedy] UnrestrictedCoursesController sut,
        CancellationToken cancellationToken)
    {
        var courses = new List<RestrictedCourseModel>
        {
            new() { LarsCode = "300", Title = "Zebra course", Level = 3, LearningType = LearningType.Apprenticeship },
            new() { LarsCode = "100", Title = "Alpha course", Level = 5, LearningType = LearningType.Apprenticeship },
            new() { LarsCode = "200", Title = "Alpha course", Level = 3, LearningType = LearningType.Apprenticeship }
        };
        serviceMock.Setup(x => x.GetUnrestrictedCourses(cancellationToken)).ReturnsAsync(courses);

        var result = await sut.Index("course", cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        var matched = result!.Value as IEnumerable<UnrestrictedCourseSearchItem>;
        matched.Should().NotBeNull();
        matched!.Select(c => c.LarsCode).Should().ContainInOrder("200", "100", "300");
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndSearchTermMatchesTitle_ThenReturnsMatchingCourses(
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

        const string searchTerm = "soft";
        var result = await sut.Index(searchTerm, cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        var matched = result!.Value as IEnumerable<UnrestrictedCourseSearchItem>;
        matched.Should().ContainSingle();
        matched!.Single().LarsCode.Should().Be("100");
        matched.Single().DisplayTitle.Should().Be("Software developer (Level 4)");
        matched.Single().Level.Should().Be(4);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndSearchTermMatchesLevel_ThenReturnsMatchingCourses(
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

        const string searchTerm = "Level 4";
        var result = await sut.Index(searchTerm, cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        var matched = result!.Value as IEnumerable<UnrestrictedCourseSearchItem>;
        matched.Should().ContainSingle();
        matched!.Single().LarsCode.Should().Be("100");
        matched.Single().DisplayTitle.Should().Be("Software developer (Level 4)");
        matched.Single().Level.Should().Be(4);
    }

    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourses_AndSearchTermMatchesLarsCodeOnly_ThenReturnsEmptyList(
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

        const string searchTerm = "123";
        var result = await sut.Index(searchTerm, cancellationToken) as OkObjectResult;

        result.Should().NotBeNull();
        var matched = result!.Value as IEnumerable<UnrestrictedCourseSearchItem>;
        matched.Should().BeEmpty();
    }
}
