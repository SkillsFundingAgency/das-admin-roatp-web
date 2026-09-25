using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/add/set-last-start-date", Name = RouteNames.ProviderRestrictedCourseSetLastStartDate)]
public class ProviderRestrictedCourseSetLastStartDateController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient,
    IValidator<SetLastDateStartsSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ProviderRestrictedCourseSetLastStartDate/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} is now restricted and has a last date for new starts";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken = default)
    {
        var session = GetProviderRestrictedCourseSession(ukprn);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
        }

        var model = await BuildViewModelAsync(session, null, cancellationToken);
        return model is null ? NotFound() : View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        SetLastDateStartsSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var session = GetProviderRestrictedCourseSession(ukprn);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
        }

        var model = await BuildViewModelAsync(session, submitModel, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        submitModel.LarsCode = session.LarsCode;
        submitModel.CourseLastDateStarts = model.CourseLastDateStarts;

        var validationResult = await validator.ValidateAsync(submitModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, model);
        }

        if (!submitModel.TryGetEnteredDate(out var lastDateStarts))
        {
            ModelState.AddModelError(
                SetLastDateStartsSubmitModelValidator.DateFieldName,
                SetLastDateStartsSubmitModelValidator.EnterValidDateErrorMessage);
            return View(ViewPath, model);
        }

        var response = await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            session.LarsCode,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = lastDateStarts,
                IsStartRestricted = true
            },
            cancellationToken);

        if (response.IsNotFound())
        {
            return NotFound();
        }

        await response.EnsureSuccessStatusCodeAsync();

        sessionService.Delete(SessionKeys.ProviderRestrictedCourse);
        TempData[RestrictedApprenticeshipsController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(session.CourseDisplayTitle);

        return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
    }

    private ProviderRestrictedCourseSessionModel? GetProviderRestrictedCourseSession(int ukprn)
    {
        var session = sessionService.Get<ProviderRestrictedCourseSessionModel>(SessionKeys.ProviderRestrictedCourse);
        if (session is null || session.Ukprn != ukprn)
        {
            return null;
        }

        return session;
    }

    private async Task<ProviderRestrictedCourseSetLastStartDateViewModel?> BuildViewModelAsync(
        ProviderRestrictedCourseSessionModel session,
        SetLastDateStartsSubmitModel? submitModel,
        CancellationToken cancellationToken)
    {
        var providerName = await TempData.GetProviderName(outerApiClient, session.Ukprn, cancellationToken);
        if (providerName is null)
        {
            return null;
        }

        return new ProviderRestrictedCourseSetLastStartDateViewModel
        {
            Ukprn = session.Ukprn,
            ProviderName = providerName,
            CourseDisplayTitle = session.CourseDisplayTitle,
            LarsCode = session.LarsCode,
            CourseLastDateStarts = await GetCourseLastDateStartsAsync(session.LarsCode, cancellationToken),
            Day = submitModel?.Day,
            Month = submitModel?.Month,
            Year = submitModel?.Year,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn = session.Ukprn })!
        };
    }

    private async Task<DateTime?> GetCourseLastDateStartsAsync(string larsCode, CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetAllowedProvidersForCourse(larsCode, cancellationToken);
        if (response.IsNotFound())
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();
        return response.Content?.LastDateStarts;
    }
}
