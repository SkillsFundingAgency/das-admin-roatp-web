using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restrict", Name = RouteNames.RestrictProvider)]
public class RestrictProviderController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RestrictProvider/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var organisationDetailsUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!;
        var viewModel = new RestrictProviderViewModel
        {
            Ukprn = ukprn,
            ProviderName = providerName,
            CancelUrl = organisationDetailsUrl,
            BackLinkUrl = organisationDetailsUrl
        };

        return View(ViewPath, viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> IndexPost(int ukprn, CancellationToken cancellationToken)
    {
        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        return RedirectToRoute(RouteNames.ConfirmRestrictProvider, new { ukprn });
    }
}
