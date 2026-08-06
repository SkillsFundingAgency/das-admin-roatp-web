using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

public class UnrestrictedCourseSearchControllerGetTests
{
    [Test, MoqAutoData]
    public void WhenGettingUnrestrictedCourseSearch_ThenReturnsViewWithModel(
        [Frozen] IValidator<UnrestrictedCourseSearchSubmitModel> validator,
        [Greedy] UnrestrictedCourseSearchController controller)
    {
        var actual = controller.Index() as ViewResult;

        actual.Should().NotBeNull();
        actual!.ViewName.Should().Be(UnrestrictedCourseSearchController.ViewPath);
        actual.Model.Should().BeOfType<UnrestrictedCourseSearchViewModel>();
    }
}
