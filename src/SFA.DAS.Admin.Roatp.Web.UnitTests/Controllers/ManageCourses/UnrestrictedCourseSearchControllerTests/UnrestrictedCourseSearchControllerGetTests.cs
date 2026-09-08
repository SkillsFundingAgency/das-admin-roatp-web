using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.UnrestrictedCourseSearchControllerTests;

public class UnrestrictedCourseSearchControllerGetTests
{
    [Test, MoqAutoData]
    public async Task WhenGettingUnrestrictedCourseSearch_ThenReturnsViewWithCourses(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] IValidator<UnrestrictedCourseSearchSubmitModel> validator,
        [Greedy] UnrestrictedCourseSearchController controller,
        List<RestrictedCourseModel> courses)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse { Courses = courses });

        var actual = await controller.Index(CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(UnrestrictedCourseSearchController.ViewPath);
            var model = actual.Model as UnrestrictedCourseSearchViewModel;
            model.Should().NotBeNull();
            model!.Courses.Should().HaveCount(courses.Count);
        }
    }
}
