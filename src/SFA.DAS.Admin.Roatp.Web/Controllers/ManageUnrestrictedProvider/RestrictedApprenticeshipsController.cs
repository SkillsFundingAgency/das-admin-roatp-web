using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses", Name = RouteNames.ProviderRestrictedCourses)]
public class RestrictedApprenticeshipsController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RestrictedApprenticeships/Index.cshtml";
    public const string ProviderLegalNameTempDataKey = "ProviderLegalName";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var providerName = await GetProviderName(ukprn, cancellationToken);
        if (providerName is null)
        {
            return RedirectToRoute(RouteNames.Home);
        }

        var apiResponse = await outerApiClient.GetRestrictedApprenticeships(ukprn, cancellationToken);
        if (apiResponse.StatusCode != HttpStatusCode.OK)
        {
            return RedirectToRoute(RouteNames.Home);
        }

        RestrictedApprenticeshipsViewModel model = apiResponse.Content!;
        model.ProviderName = providerName;
        model.BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!;
        model.RestrictACourseUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!;

        return View(ViewPath, model);
    }

    private async Task<string?> GetProviderName(int ukprn, CancellationToken cancellationToken)
    {
        var cachedProviderName = TempData.Peek(ProviderLegalNameTempDataKey) as string;
        if (!string.IsNullOrWhiteSpace(cachedProviderName))
        {
            TempData.Keep(ProviderLegalNameTempDataKey);
            return cachedProviderName;
        }

        var organisationApiResponse = await outerApiClient.GetOrganisation(ukprn, cancellationToken);
        if (organisationApiResponse.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var providerName = organisationApiResponse.Content!.LegalName;
        TempData[ProviderLegalNameTempDataKey] = providerName;
        return providerName;
    }
}
