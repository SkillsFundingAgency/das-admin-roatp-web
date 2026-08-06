using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

public class SearchCourseToRestrictControllerGetTests
{
    [Test, MoqAutoData]
    public void WhenGettingSearchCourseToRestrict_ThenReturnsViewWithModel(
        [Frozen] IValidator<SearchCourseToRestrictViewModel> validator,
        [Greedy] SearchCourseToRestrictController controller)
    {
        var actual = controller.Index() as ViewResult;

        actual.Should().NotBeNull();
        actual!.ViewName.Should().Be(SearchCourseToRestrictController.ViewPath);
        actual.Model.Should().BeOfType<SearchCourseToRestrictViewModel>();
    }
}
