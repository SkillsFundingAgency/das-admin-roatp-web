using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses/{larsCode}/change-restriction/remove-restriction",
    Name = RouteNames.RemoveRestrictedCourseRestriction)]
public class RemoveRestrictedCourseRestrictionController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient,
    IValidator<ChangeRestrictedCourseRestrictionSessionModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RemoveRestrictedCourseRestriction/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} has been removed from the restricted apprenticeships list";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, string larsCode)
    {
        var session = GetChangeRestrictedCourseSession(ukprn, larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        if (!await IsValidForRemoveRestriction(session, HttpContext.RequestAborted))
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(session));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        int ukprn,
        string larsCode,
        CancellationToken cancellationToken)
    {
        var session = GetChangeRestrictedCourseSession(ukprn, larsCode);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
        }

        var response = await outerApiClient.RemoveRestrictedApprenticeship(
            ukprn,
            larsCode,
            new RemoveRestrictedApprenticeshipRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName()
            },
            cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

        sessionService.Delete(SessionKeys.RestrictedCourseChangeRestriction);

        TempData[ProviderRestrictedApprenticeshipsController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(session.CourseDisplayTitle);

        return RedirectToRoute(RouteNames.ProviderRestrictedApprenticeships, new { ukprn });
    }

    private async Task<bool> IsValidForRemoveRestriction(
        ChangeRestrictedCourseRestrictionSessionModel session,
        CancellationToken cancellationToken)
    {
        var validateResult = await validator.ValidateAsync(session, cancellationToken);
        return validateResult.IsValid;
    }

    private ChangeRestrictedCourseRestrictionSessionModel? GetChangeRestrictedCourseSession(int ukprn, string larsCode)
    {
        var model = sessionService.Get<ChangeRestrictedCourseRestrictionSessionModel>(
            SessionKeys.RestrictedCourseChangeRestriction);
        if (model is null || model.Ukprn != ukprn || model.LarsCode != larsCode)
        {
            return null;
        }

        return model;
    }

    private RemoveRestrictedCourseRestrictionViewModel BuildViewModel(
        ChangeRestrictedCourseRestrictionSessionModel session)
        => new()
        {
            Ukprn = session.Ukprn,
            LarsCode = session.LarsCode,
            CourseDisplayTitle = session.CourseDisplayTitle,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedApprenticeships, new { ukprn = session.Ukprn })!
        };
}
