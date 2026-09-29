using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
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
    IValidator<ProviderRestrictedCourseSearchSubmitModel> validator,
    IApplicationCacheService applicationCacheService) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ProviderRestrictedCourseSearch/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.ProviderRestrictedCourse);

        var response = await GetNotRestrictedApprenticeships(ukprn, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(ukprn, response.Courses ?? []));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        ProviderRestrictedCourseSearchSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            var coursesResponse = await GetNotRestrictedApprenticeships(ukprn, cancellationToken);
            return View(ViewPath, BuildViewModel(ukprn, coursesResponse?.Courses ?? []));
        }

        var course = await GetCourse(submitModel.SelectedLarsCode!, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        sessionService.Set(SessionKeys.ProviderRestrictedCourse, new ProviderRestrictedCourseSessionModel
        {
            Ukprn = ukprn,
            LarsCode = submitModel.SelectedLarsCode!,
            Title = course.Title,
            Level = course.Level,
            CourseDisplayTitle = CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level),
            ProviderName = providerName
        });

        var hasProviderCourse = await HasProviderCourse(ukprn, submitModel.SelectedLarsCode!, cancellationToken);

        return RedirectToRoute(
            hasProviderCourse
                ? RouteNames.ProviderRestrictedCourseSetLastStartDate
                : RouteNames.ConfirmProviderRestrictedCourse,
            new { ukprn });
    }

    private async Task<GetCourseResponse?> GetCourse(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var courses = await GetCourses(cancellationToken);
        return courses?.FirstOrDefault(course => course.LarsCode == larsCode);
    }

    private async Task<List<GetCourseResponse>?> GetCourses(CancellationToken cancellationToken)
    {
        var cached = await applicationCacheService.GetAsync<GetCoursesResponse>(ApplicationCacheKeys.CoursesCacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached.Courses;
        }

        var response = await outerApiClient.GetCourses(cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        if (response.Content is not null)
        {
            await applicationCacheService.SetAsync(ApplicationCacheKeys.CoursesCacheKey, response.Content, cancellationToken: cancellationToken);
        }

        return response.Content?.Courses;
    }

    private async Task<bool> HasProviderCourse(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetProviderCourse(ukprn, larsCode, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return true;
    }

    private async Task<GetNotRestrictedApprenticeshipsResponse?> GetNotRestrictedApprenticeships(
        int ukprn,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetNotRestrictedApprenticeships(ukprn, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return response.Content ?? new GetNotRestrictedApprenticeshipsResponse();
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
