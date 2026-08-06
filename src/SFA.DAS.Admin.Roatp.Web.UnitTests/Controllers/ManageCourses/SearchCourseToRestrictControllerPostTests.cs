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

public class SearchCourseToRestrictControllerPostTests
{
    [Test, MoqAutoData]
    public void WhenPostingSearchCourseToRestrict_AndModelIsValid_ThenRedirectsToSearchPage(
        SearchCourseToRestrictViewModel viewModel,
        [Frozen] Mock<IValidator<SearchCourseToRestrictViewModel>> validator,
        [Greedy] SearchCourseToRestrictController controller)
    {
        validator.Setup(x => x.Validate(viewModel)).Returns(new ValidationResult());

        var actual = controller.Index(viewModel);

        actual.Should().NotBeNull();
        var result = actual as RedirectToRouteResult;
        result.Should().NotBeNull();
        result!.RouteName.Should().Be(RouteNames.SearchCourseToRestrict);
    }

    [Test, MoqAutoData]
    public void WhenPostingSearchCourseToRestrict_AndModelIsInvalid_ThenReloadsView(
        SearchCourseToRestrictViewModel viewModel,
        [Frozen] Mock<IValidator<SearchCourseToRestrictViewModel>> validator,
        [Greedy] SearchCourseToRestrictController controller)
    {
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("SearchTerm", "Error"));

        validator.Setup(x => x.Validate(viewModel)).Returns(validationResult);

        var actual = controller.Index(viewModel);

        actual.Should().NotBeNull();
        var result = actual as ViewResult;
        result.Should().NotBeNull();
        result!.ViewName.Should().Be(SearchCourseToRestrictController.ViewPath);
        result.Model.Should().BeOfType<SearchCourseToRestrictViewModel>();
    }
}
