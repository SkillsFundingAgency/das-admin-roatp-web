using AutoFixture.NUnit4;
using FluentAssertions;
using Moq;
using SFA.DAS.Admin.Roatp.Application.RestrictedCourses.Queries.GetRestrictedCourses;
using SFA.DAS.Admin.Roatp.Domain.Interfaces;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.UnitTests.Application.RestrictedCourses.Queries.GetRestrictedCourses;

[TestFixture]
public class GetRestrictedCoursesQueryHandlerTests
{
    [Test, MoqAutoData]
    public async Task Handle_WhenNoFilters_ReturnsAllCourses(
        [Frozen] Mock<IRestrictedCoursesApiClient> outerApiClientMock,
        [Greedy] GetRestrictedCoursesQueryHandler sut)
    {
        var apiResponse = new GetRestrictedCoursesResponse
        {
            Courses =
            [
                new RestrictedCourseModel { LarsCode = "1", Title = "Bricklaying", Level = 2, LearningType = LearningType.Apprenticeship },
                new RestrictedCourseModel { LarsCode = "2", Title = "Engineering", Level = 3, LearningType = LearningType.ApprenticeshipUnit }
            ],
            TotalCount = 2
        };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        var result = await sut.Handle(new GetRestrictedCoursesQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Courses.Should().BeEquivalentTo(apiResponse.Courses);
    }

    [Test, MoqAutoData]
    public async Task Handle_WhenCourseNameProvided_FiltersByTitleOrLarsCode(
        [Frozen] Mock<IRestrictedCoursesApiClient> outerApiClientMock,
        [Greedy] GetRestrictedCoursesQueryHandler sut)
    {
        var matchingByTitle = new RestrictedCourseModel { LarsCode = "10", Title = "Engineering maintenance", Level = 3, LearningType = LearningType.Apprenticeship };
        var matchingByLars = new RestrictedCourseModel { LarsCode = "192", Title = "Bricklaying", Level = 2, LearningType = LearningType.Apprenticeship };
        var nonMatching = new RestrictedCourseModel { LarsCode = "50", Title = "Painting", Level = 3, LearningType = LearningType.Apprenticeship };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse
            {
                Courses = [matchingByTitle, matchingByLars, nonMatching],
                TotalCount = 3
            });

        var resultByTitle = await sut.Handle(new GetRestrictedCoursesQuery { CourseName = "Engineering" }, CancellationToken.None);
        resultByTitle.Courses.Should().BeEquivalentTo([matchingByTitle]);
        resultByTitle.TotalCount.Should().Be(1);

        var resultByLars = await sut.Handle(new GetRestrictedCoursesQuery { CourseName = "192" }, CancellationToken.None);
        resultByLars.Courses.Should().BeEquivalentTo([matchingByLars]);
        resultByLars.TotalCount.Should().Be(1);
    }

    [Test, MoqAutoData]
    public async Task Handle_WhenLearningTypesProvided_FiltersByLearningType(
        [Frozen] Mock<IRestrictedCoursesApiClient> outerApiClientMock,
        [Greedy] GetRestrictedCoursesQueryHandler sut)
    {
        var apprenticeship = new RestrictedCourseModel { LarsCode = "1", Title = "Bricklaying", Level = 2, LearningType = LearningType.Apprenticeship };
        var unit = new RestrictedCourseModel { LarsCode = "2", Title = "Engineering unit", Level = 3, LearningType = LearningType.ApprenticeshipUnit };
        var foundation = new RestrictedCourseModel { LarsCode = "3", Title = "Foundation course", Level = 2, LearningType = LearningType.FoundationApprenticeship };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse
            {
                Courses = [apprenticeship, unit, foundation],
                TotalCount = 3
            });

        var result = await sut.Handle(new GetRestrictedCoursesQuery
        {
            LearningTypes = [LearningType.ApprenticeshipUnit]
        }, CancellationToken.None);

        result.Courses.Should().BeEquivalentTo([unit]);
        result.TotalCount.Should().Be(1);
    }

    [Test, MoqAutoData]
    public async Task Handle_WhenCourseNameAndLearningTypesProvided_AppliesBothFilters(
        [Frozen] Mock<IRestrictedCoursesApiClient> outerApiClientMock,
        [Greedy] GetRestrictedCoursesQueryHandler sut)
    {
        var match = new RestrictedCourseModel { LarsCode = "192", Title = "Engineering maintenance", Level = 3, LearningType = LearningType.ApprenticeshipUnit };
        var wrongType = new RestrictedCourseModel { LarsCode = "193", Title = "Engineering foundation", Level = 2, LearningType = LearningType.FoundationApprenticeship };
        var wrongName = new RestrictedCourseModel { LarsCode = "50", Title = "Painting", Level = 3, LearningType = LearningType.ApprenticeshipUnit };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse
            {
                Courses = [match, wrongType, wrongName],
                TotalCount = 3
            });

        var result = await sut.Handle(new GetRestrictedCoursesQuery
        {
            CourseName = "Engineering",
            LearningTypes = [LearningType.ApprenticeshipUnit]
        }, CancellationToken.None);

        result.Courses.Should().BeEquivalentTo([match]);
        result.TotalCount.Should().Be(1);
    }
}
