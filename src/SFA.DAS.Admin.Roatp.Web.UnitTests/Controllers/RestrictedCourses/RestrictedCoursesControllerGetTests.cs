using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.RestrictedCourses;

[TestFixture]
public class RestrictedCoursesControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Index_ReturnsViewWithMappedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RestrictedCoursesController sut)
    {
        var response = new GetRestrictedCoursesResponse
        {
            Courses =
            [
                new RestrictedCourseModel
                {
                    LarsCode = "124",
                    Title = "Bricklaying",
                    Level = 2,
                    LearningType = LearningType.Apprenticeship
                }
            ]
        };

        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock();

        var result = await sut.Index(CancellationToken.None) as ViewResult;

        result.Should().NotBeNull();
        var model = result!.Model as RestrictedCoursesViewModel;
        model.Should().NotBeNull();
        model!.TotalCount.Should().Be(response.Courses.Count);
        model.TotalCountDescription.Should().Be("1 course");
        model.HasCourses.Should().BeTrue();
        model.Courses.Should().HaveCount(1);
        var course = model.Courses.Single();
        course.LarsCode.Should().Be("124");
        course.DisplayTitle.Should().Be("Bricklaying (Level 2)");
        course.LearningTypeDescription.Should().Be("Apprenticeship");
        course.LearningTypeTagClass.Should().Be("govuk-tag--blue");

        outerApiClientMock.Verify(c => c.GetRestrictedCourses(true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
