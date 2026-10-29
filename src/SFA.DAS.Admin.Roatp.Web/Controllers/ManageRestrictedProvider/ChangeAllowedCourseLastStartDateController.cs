using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/{routeLearningType}/allowed-courses/{larsCode}/change", Name = RouteNames.ChangeAllowedCourseLastStartDate)]
public class ChangeAllowedCourseLastStartDateController(
    IOuterApiClient outerApiClient,
    IUkprnService ukprnService,
    ICoursesService coursesService,
    IValidator<ChangeAllowedCourseLastStartDateSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageRestrictedProvider/ChangeAllowedCourseLastStartDate/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
        string larsCode,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(routeLearningType))
        {
            return NotFound();
        }

        var organisation = await ukprnService.GetOrganisationAsync(ukprn, cancellationToken);
        if (organisation is null)
        {
            return NotFound();
        }

        if (organisation.IsUnrestrictedProvider(routeLearningType))
        {
            return NotFound();
        }

        ProviderAllowedCourseModel? allowedCourse = await outerApiClient.GetAllowedCourse(ukprn, larsCode, cancellationToken);
        if (allowedCourse is null)
        {
            return NotFound();
        }
        if (!allowedCourse.LastDateStarts.HasValue)
        {
            return RedirectToRoute(RouteNames.AddAllowedCourseLastStartDate, new { ukprn, routeLearningType, larsCode });
        }

        var course = await coursesService.GetCourse(larsCode, cancellationToken);
        if (course is null || course.CourseType != routeLearningType.ToCourseType())
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(ukprn, routeLearningType, course, allowedCourse.LastDateStarts.Value));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
        string larsCode,
        ChangeAllowedCourseLastStartDateSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        ProviderAllowedCourseModel? allowedCourse = await outerApiClient.GetAllowedCourse(ukprn, larsCode, cancellationToken);
        if (allowedCourse is null)
        {
            return NotFound();
        }

        var course = await coursesService.GetCourse(larsCode, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(
                ViewPath,
                BuildViewModel(ukprn, routeLearningType, course, allowedCourse.LastDateStarts!.Value, submitModel));
        }

        if (submitModel.SelectedOption == ChangeAllowedCourseLastStartDateOptions.Change)
        {
            return RedirectToRoute(RouteNames.EditAllowedCourseLastStartDate, new { ukprn, routeLearningType, larsCode });
        }

        return RedirectToRoute(RouteNames.ChangeAllowedCourseLastStartDate, new { ukprn, routeLearningType, larsCode });
    }

    private ChangeAllowedCourseLastStartDateViewModel BuildViewModel(
        int ukprn,
        RouteLearningType routeLearningType,
        GetCourseResponse course,
        DateTime lastDateStarts,
        ChangeAllowedCourseLastStartDateSubmitModel? submitModel = null)
        => new()
        {
            Ukprn = ukprn,
            RouteLearningType = routeLearningType,
            LarsCode = course.LarsCode,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
            LastDateStarts = lastDateStarts,
            SelectedOption = submitModel?.SelectedOption,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType })!
        };
}
