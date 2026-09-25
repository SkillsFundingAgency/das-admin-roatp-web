using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/add", Name = RouteNames.ProviderRestrictedCourseSearch)]
public class ProviderRestrictedCourseSearchController(
    IOuterApiClient outerApiClient,
    ISessionService sessionService,
    IValidator<ProviderRestrictedCourseSearchSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ProviderRestrictedCourseSearch/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.ProviderRestrictedCourse);

        var courses = await GetSearchableCoursesAsync(ukprn, cancellationToken);
        if (courses is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(ukprn, courses));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        ProviderRestrictedCourseSearchSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var courses = await GetSearchableCoursesAsync(ukprn, cancellationToken);

        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, BuildViewModel(ukprn, courses ?? []));
        }

        var course = courses?.FirstOrDefault(c => c.LarsCode == submitModel.SelectedLarsCode);
        if (course is null)
        {
            return NotFound();
        }

        var providerCourseResponse = await outerApiClient.GetProviderCourse(
            ukprn,
            course.LarsCode,
            cancellationToken);
        if (!providerCourseResponse.IsNotFound())
        {
            await providerCourseResponse.EnsureSuccessStatusCodeAsync();
        }

        sessionService.Set(SessionKeys.ProviderRestrictedCourse, new ProviderRestrictedCourseSessionModel
        {
            Ukprn = ukprn,
            LarsCode = course.LarsCode,
            Title = course.Title,
            Level = course.Level,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level)
        });

        return RedirectToRoute(
            providerCourseResponse.IsNotFound()
                ? RouteNames.ConfirmProviderRestrictedCourse
                : RouteNames.ProviderRestrictedCourseSetLastStartDate,
            new { ukprn });
    }

    private async Task<List<NotRestrictedApprenticeshipModel>?> GetSearchableCoursesAsync(
        int ukprn,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetNotRestrictedApprenticeships(ukprn, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return response.Content?.Courses ?? [];
    }

    private static ProviderRestrictedCourseSearchViewModel BuildViewModel(
        int ukprn,
        IEnumerable<NotRestrictedApprenticeshipModel> courses,
        string? selectedLarsCode = null)
        => new()
        {
            Ukprn = ukprn,
            SelectedLarsCode = selectedLarsCode,
            Courses = courses
                .OrderBy(
                    course => CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                    StringComparer.OrdinalIgnoreCase)
                .Select(course => new SelectListItem(
                    CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
                    course.LarsCode,
                    course.LarsCode == selectedLarsCode))
        };
}
