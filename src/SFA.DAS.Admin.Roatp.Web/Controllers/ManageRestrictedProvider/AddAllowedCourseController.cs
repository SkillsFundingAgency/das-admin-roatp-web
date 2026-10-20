using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/{routeLearningType}/allowed-courses/add", Name = RouteNames.AddAllowedCourse)]
public class AddAllowedCourseController(
    IOuterApiClient outerApiClient,
    IUkprnService ukprnService,
    ICoursesService coursesService,
    IValidator<AddAllowedCourseSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageRestrictedProvider/AddAllowedCourse/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} added to approved list";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(routeLearningType))
        {
            return NotFound();
        }

        GetOrganisationResponse? organisation = await ukprnService.GetOrganisationAsync(ukprn, cancellationToken);
        if (organisation is null)
        {
            return NotFound();
        }

        if (organisation.IsUnrestrictedProvider(routeLearningType))
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        GetCoursesToBeAllowedResponse coursesToBeAllowedResponse = await outerApiClient.GetCoursesToBeAllowed(ukprn, routeLearningType.ToCourseType(), cancellationToken);

        return View(ViewPath, BuildViewModel(ukprn, routeLearningType, organisation.LegalName, coursesToBeAllowedResponse.Courses ?? []));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        RouteLearningType routeLearningType,
        AddAllowedCourseSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            GetOrganisationResponse? organisation = await ukprnService.GetOrganisationAsync(ukprn, cancellationToken);

            GetCoursesToBeAllowedResponse coursesToBeAllowedResponse = await outerApiClient.GetCoursesToBeAllowed(ukprn, routeLearningType.ToCourseType(), cancellationToken);
            return View(
                ViewPath,
                BuildViewModel(
                    ukprn,
                    routeLearningType,
                    organisation!.LegalName,
                    coursesToBeAllowedResponse?.Courses ?? [],
                    submitModel.SelectedLarsCode));
        }

        var course = await coursesService.GetCourse(submitModel.SelectedLarsCode!, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        var response = await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            submitModel.SelectedLarsCode!,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = null,
                IsStartRestricted = false
            },
            cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

        TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(CourseDisplayModelExtensions.GetDisplayTitle(course.Title, course.Level));

        return RedirectToRoute(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType });
    }

    private AddAllowedCourseViewModel BuildViewModel(
        int ukprn,
        RouteLearningType routeLearningType,
        string providerName,
        IEnumerable<CourseToBeAllowedModel> courses,
        string? selectedLarsCode = null)
        => new()
        {
            Ukprn = ukprn,
            RouteLearningType = routeLearningType,
            ProviderName = providerName,
            SelectedLarsCode = selectedLarsCode,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderAllowedCourses, new { ukprn, routeLearningType })!,
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
