using AutoFixture.NUnit4;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Application.RestrictedCourses.Queries.GetRestrictedCourses;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.RestrictedCourses;

[TestFixture]
public class RestrictedCoursesControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Index_ReturnsViewWithMappedModel(
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] RestrictedCoursesController sut,
        GetRestrictedCoursesQueryResult queryResult)
    {
        queryResult.TotalCount = 16;
        queryResult.Courses =
        [
            new RestrictedCourseModel
            {
                LarsCode = 124,
                Title = "Bricklaying",
                Level = 2,
                LearningType = LearningType.Apprenticeship
            }
        ];

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRestrictedCoursesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        sut.AddUrlHelperMock();

        var result = await sut.Index(CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        var model = result!.Model as RestrictedCoursesViewModel;
        model.Should().NotBeNull();
        model!.TotalCount.Should().Be(16);
        model.TotalCountDescription.Should().Be("16 courses");
        model.Courses.Should().HaveCount(1);
        model.Courses[0].LarsCode.Should().Be(124);
        model.Courses[0].DisplayTitle.Should().Be("Bricklaying (Level 2)");
        model.Courses[0].LearningTypeDescription.Should().Be("Apprenticeship");
        model.Courses[0].LearningTypeTagClass.Should().Be("govuk-tag--blue");

        mediatorMock.Verify(m => m.Send(
            It.IsAny<GetRestrictedCoursesQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
