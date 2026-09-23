using System.Net;
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
[Route("providers/{ukprn}/restricted-courses/add/confirm", Name = RouteNames.ConfirmRestrictCourse)]
public class ConfirmRestrictCourseController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ConfirmRestrictCourse/Index.cshtml";

    public static string GetSuccessBannerMessage(string displayTitle) =>
        $"{displayTitle} has been added to the restricted apprenticeships list";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn)
    {
        var session = GetSession(ukprn);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
        }

        var providerName = await GetProviderName(ukprn, CancellationToken.None);
        if (providerName is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(session, providerName));
    }

    [HttpPost]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var session = GetSession(ukprn);
        if (session is null)
        {
            return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
        }

        var response = await outerApiClient.UpsertProviderAllowedCourse(
            ukprn,
            session.LarsCode,
            new UpsertProviderAllowedCourseRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName(),
                LastDateStarts = null,
                IsStartRestricted = true
            },
            cancellationToken);

        if (response.IsNotFound())
        {
            return NotFound();
        }

        await response.EnsureSuccessStatusCodeAsync();

        sessionService.Delete(SessionKeys.RestrictCourse);
        TempData[RestrictedApprenticeshipsController.SuccessBannerTempDataKey] =
            GetSuccessBannerMessage(session.DisplayTitle);

        return RedirectToRoute(RouteNames.ProviderRestrictedCourses, new { ukprn });
    }

    private RestrictCourseSessionModel? GetSession(int ukprn)
    {
        var session = sessionService.Get<RestrictCourseSessionModel>(SessionKeys.RestrictCourse);
        if (session is null || session.Ukprn != ukprn)
        {
            return null;
        }

        return session;
    }

    private async Task<string?> GetProviderName(int ukprn, CancellationToken cancellationToken)
    {
        var cachedProviderName = TempData.Peek(TempDataKeys.ProviderLegalName) as string;
        if (!string.IsNullOrWhiteSpace(cachedProviderName))
        {
            TempData.Keep(TempDataKeys.ProviderLegalName);
            return cachedProviderName;
        }

        var organisationApiResponse = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationApiResponse.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var providerName = organisationApiResponse.Content!.LegalName;
        TempData[TempDataKeys.ProviderLegalName] = providerName;
        return providerName;
    }

    private ConfirmRestrictCourseViewModel BuildViewModel(RestrictCourseSessionModel session, string providerName)
        => new()
        {
            Ukprn = session.Ukprn,
            ProviderName = providerName,
            DisplayTitle = session.DisplayTitle,
            LarsCode = session.LarsCode,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn = session.Ukprn })!
        };
}
