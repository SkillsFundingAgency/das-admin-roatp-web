using AutoFixture.NUnit4;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;
using SFA.DAS.Admin.Roatp.Web.Validators;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.Controllers.ManageUnrestrictedProvider.ChangeProviderRestrictedCourseControllerTests;

[TestFixture]
public class ChangeProviderRestrictedCourseControllerPostTests
{
    private const int Ukprn = 10019900;
    private const string LarsCode = "105";
    private const string RestrictedCoursesUrl = "/providers/10019900/restricted-courses";
    private static readonly DateTime LastDateStarts = new(2026, 7, 12, 0, 0, 0, DateTimeKind.Unspecified);

    [Test, MoqAutoData]
    public void WhenPostingChange_AndNoOptionIsSelected_ThenReloadsViewWithError(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Frozen] Mock<IValidator<ChangeProviderRestrictedCourseSubmitModel>> validatorMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCachedCourse(applicationCacheMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ChangeProviderRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult(
            [
                new ValidationFailure(
                    nameof(ChangeProviderRestrictedCourseSubmitModel.SelectedOption),
                    ChangeProviderRestrictedCourseSubmitModelValidator.NoOptionSelectedErrorMessage)
            ]));
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = sut.Index(Ukprn, LarsCode, new ChangeProviderRestrictedCourseSubmitModel()) as ViewResult;
        var model = result?.Model as ChangeProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.DisplayTitle.Should().Be("Electrical (Level 3)");
            sut.ModelState.IsValid.Should().BeFalse();
        }
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndAddOrChangeIsSelected_ThenRefreshesPage(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Frozen] Mock<IValidator<ChangeProviderRestrictedCourseSubmitModel>> validatorMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCachedCourse(applicationCacheMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ChangeProviderRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new ChangeProviderRestrictedCourseSubmitModel
            {
                SelectedOption = ChangeProviderRestrictedCourseOptions.AddOrChange
            }) as ViewResult;
        var model = result?.Model as ChangeProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.SelectedOption.Should().Be(ChangeProviderRestrictedCourseOptions.AddOrChange);
            sut.ModelState.IsValid.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndRemoveIsSelected_ThenRefreshesPage(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Frozen] Mock<IValidator<ChangeProviderRestrictedCourseSubmitModel>> validatorMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        SetupCachedCourse(applicationCacheMock);
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ChangeProviderRestrictedCourseSubmitModel>()))
            .Returns(new ValidationResult());
        sut.AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.ProviderRestrictedCourses, RestrictedCoursesUrl);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new ChangeProviderRestrictedCourseSubmitModel
            {
                SelectedOption = ChangeProviderRestrictedCourseOptions.Remove
            }) as ViewResult;
        var model = result?.Model as ChangeProviderRestrictedCourseViewModel;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ViewName.Should().Be(ChangeProviderRestrictedCourseController.ViewPath);
            model.Should().NotBeNull();
            model!.SelectedOption.Should().Be(ChangeProviderRestrictedCourseOptions.Remove);
            sut.ModelState.IsValid.Should().BeTrue();
        }
    }

    [Test, MoqAutoData]
    public void WhenPostingChange_AndCacheIsMissing_ThenRedirectsToRestrictedCoursesList(
        [Frozen] Mock<IApplicationCacheService> applicationCacheMock,
        [Greedy] ChangeProviderRestrictedCourseController sut)
    {
        List<RestrictedApprenticeshipModel>? courses = null;
        applicationCacheMock
            .Setup(c => c.TryGet(ApplicationCacheKeys.RestrictedApprenticeships(Ukprn), out courses))
            .Returns(false);

        var result = sut.Index(
            Ukprn,
            LarsCode,
            new ChangeProviderRestrictedCourseSubmitModel
            {
                SelectedOption = ChangeProviderRestrictedCourseOptions.AddOrChange
            }) as RedirectToRouteResult;

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(RouteNames.ProviderRestrictedCourses);
            result.RouteValues!["ukprn"].Should().Be(Ukprn);
        }
    }

    private static void SetupCachedCourse(Mock<IApplicationCacheService> applicationCacheMock)
    {
        List<RestrictedApprenticeshipModel>? courses =
        [
            new RestrictedApprenticeshipModel
            {
                LarsCode = LarsCode,
                Title = "Electrical",
                Level = 3,
                LastDateStarts = LastDateStarts,
                IsClosedToNewStarts = false
            }
        ];

        applicationCacheMock
            .Setup(c => c.TryGet(ApplicationCacheKeys.RestrictedApprenticeships(Ukprn), out courses))
            .Returns(true);
    }
}
