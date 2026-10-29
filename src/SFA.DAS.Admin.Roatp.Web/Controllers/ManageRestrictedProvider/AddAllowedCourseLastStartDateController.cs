using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/{routeLearningType}/allowed-courses/{larsCode}/set-last-date-start", Name = RouteNames.AddAllowedCourseLastStartDate)]
public class AddAllowedCourseLastStartDateController(
    IOuterApiClient outerApiClient,
    IUkprnService ukprnService,
    ICoursesService coursesService,
    IValidator<SetLastDateStartsSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageRestrictedProvider/AddAllowedCourseLastStartDate/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"Last start date added for {displayTitle}";

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

        ProviderAllowedCourseModel? allowedCourse = await GetAllowedCourse(ukprn, larsCode, cancellationToken);
        if (allowedCourse is null)
        {
            return NotFound();
        }

        var course = await coursesService.GetCourse(larsCode, cancellationToken);
        if (course is null || course.CourseType != routeLearningType.ToCourseType())
        {
            return NotFound();
        }
        if (allowedCourse.LastDateStarts.HasValue)
        {
            return RedirectToRoute(RouteNames.ChangeAllowedCourseLastStartDate, new { ukprn, routeLearningType, larsCode });
        }

        return View(ViewPath, BuildViewModel(ukprn, routeLearningType, course));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
        string larsCode,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var course = await coursesService.GetCourse(larsCode, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        submitModel.LarsCode = larsCode;
        submitModel.CourseLastDateStarts = course.LastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, BuildViewModel(ukprn, routeLearningType, course, submitModel.Day, submitModel.Month, submitModel.Year));
        }

        var response = await outerApiClient.PutProviderAllowedCourse(
            ukprn,
            larsCode,
            new PutProviderAllowedCourseRequest(submitModel.GetEnteredDate(), User.UserId(), User.UserDisplayName()),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        await response.EnsureSuccessStatusCodeAsync();

        TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level));

        return RedirectToRoute(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType });
    }

    private async Task<ProviderAllowedCourseModel?> GetAllowedCourse(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var apiResponse = await outerApiClient.GetProviderAllowedCourse(ukprn, larsCode, cancellationToken);
        if (apiResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await apiResponse.EnsureSuccessStatusCodeAsync();
        return apiResponse.Content;
    }

    private AddAllowedCourseLastStartDateViewModel BuildViewModel(
       int ukprn,
       RouteLearningType routeLearningType,
       GetCourseResponse course,
       string? day = null,
       string? month = null,
       string? year = null)
       => new()
       {
           Ukprn = ukprn,
           RouteLearningType = routeLearningType,
           CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
           LarsCode = course.LarsCode,
           CourseLastDateStarts = course.LastDateStarts,
           Day = day,
           Month = month,
           Year = year,
           CancelUrl = Url.RouteUrl(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType })!
       };
}
