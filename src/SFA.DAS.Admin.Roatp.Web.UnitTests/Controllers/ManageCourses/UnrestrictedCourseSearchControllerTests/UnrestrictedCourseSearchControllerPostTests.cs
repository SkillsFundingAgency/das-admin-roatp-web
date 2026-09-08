using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageCourses.UnrestrictedCourseSearchControllerTests;

public class UnrestrictedCourseSearchControllerPostTests
{
    private const string LarsCode = "123";
    private const string Title = "Software developer";
    private const int Level = 4;

    [Test, MoqAutoData]
    public async Task WhenPostingUnrestrictedCourseSearch_AndModelIsValid_ThenRedirectsToCourseDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<UnrestrictedCourseSearchSubmitModel>> validator,
        [Greedy] UnrestrictedCourseSearchController controller)
    {
        SetupCourses(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<UnrestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult());

        var actual = await controller.Index(
            new UnrestrictedCourseSearchSubmitModel { SelectedLarsCode = LarsCode },
            CancellationToken.None);

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            var result = actual as RedirectToRouteResult;
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.UnrestrictedCourseDetails);
            result.RouteValues!["larsCode"].Should().Be(LarsCode);
        }
    }

    [Test, MoqAutoData]
    public async Task WhenPostingUnrestrictedCourseSearch_AndModelIsInvalid_ThenReloadsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<UnrestrictedCourseSearchSubmitModel>> validator,
        [Greedy] UnrestrictedCourseSearchController controller)
    {
        SetupCourses(outerApiClientMock);
        validator.Setup(x => x.Validate(It.IsAny<UnrestrictedCourseSearchSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(UnrestrictedCourseSearchSubmitModel.SelectedLarsCode),
                    "Error")
            ]));

        var actual = await controller.Index(
            new UnrestrictedCourseSearchSubmitModel(),
            CancellationToken.None) as ViewResult;

        using (new AssertionScope())
        {
            actual.Should().NotBeNull();
            actual!.ViewName.Should().Be(UnrestrictedCourseSearchController.ViewPath);
            var model = actual.Model as UnrestrictedCourseSearchViewModel;
            model.Should().NotBeNull();
            model!.Courses.Should().HaveCount(1);
            controller.ModelState.IsValid.Should().BeFalse();
        }
    }

    private static void SetupCourses(Mock<IOuterApiClient> outerApiClientMock)
    {
        outerApiClientMock
            .Setup(c => c.GetRestrictedCourses(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetRestrictedCoursesResponse
            {
                Courses =
                [
                    new RestrictedCourseModel
                    {
                        LarsCode = LarsCode,
                        Title = Title,
                        Level = Level
                    }
                ]
            });
    }
}
