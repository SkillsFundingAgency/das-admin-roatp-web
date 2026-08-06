using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses;

public class UnrestrictedCourseSearchControllerPostTests
{
    [Test, MoqAutoData]
    public void WhenPostingUnrestrictedCourseSearch_AndModelIsValid_ThenRedirectsToSearchPage(
        UnrestrictedCourseSearchViewModel viewModel,
        [Frozen] Mock<IValidator<UnrestrictedCourseSearchViewModel>> validator,
        [Greedy] UnrestrictedCourseSearchController controller)
    {
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());

        var actual = controller.Index(viewModel);

        actual.Should().NotBeNull();
        var result = actual as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.UnrestrictedCourseSearch);
    }

    [Test, MoqAutoData]
    public void WhenPostingUnrestrictedCourseSearch_AndModelIsInvalid_ThenReloadsView(
        UnrestrictedCourseSearchViewModel viewModel,
        [Frozen] Mock<IValidator<UnrestrictedCourseSearchViewModel>> validator,
        [Greedy] UnrestrictedCourseSearchController controller)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("SearchTerm", "Error"));

        validator.Setup(x => x.Validate(viewModel)).Returns(validationResult);

        var actual = controller.Index(viewModel);

        actual.Should().NotBeNull();
        var result = actual as ViewResult;
        result.Should().NotBeNull();
        result!.ViewName.Should().Be(UnrestrictedCourseSearchController.ViewPath);
        result.Model.Should().BeOfType<UnrestrictedCourseSearchViewModel>();
    }
}
