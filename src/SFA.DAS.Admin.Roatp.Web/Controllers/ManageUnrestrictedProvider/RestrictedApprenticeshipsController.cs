using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restricted-courses", Name = RouteNames.ProviderRestrictedCourses)]
public class RestrictedApprenticeshipsController(IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/RestrictedApprenticeships/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(
        int ukprn,
        GetRestrictedApprenticeshipsRequest request,
        CancellationToken cancellationToken)
    {
        var providerName = await GetProviderName(ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var apiResponse = await outerApiClient.GetRestrictedApprenticeships(ukprn, cancellationToken);
        if (apiResponse.StatusCode != HttpStatusCode.OK)
        {
            return NotFound();
        }

        RestrictedApprenticeshipsViewModel model = apiResponse.Content!;
        model.ProviderName = providerName;
        model.BackLinkUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!;
        model.RestrictACourseUrl = Url.RouteUrl(RouteNames.ProviderRestrictedCourses, new { ukprn })!;
        model.HasActiveFilters = request.HasFilters;
        model.Filters = RestrictedApprenticeshipsFilterBuilder.CreateFiltersViewModel(request, ukprn, Url);
        model.Courses = RestrictedApprenticeshipsFilterBuilder
            .ApplyFilters(model.Courses, request)
            .ToList();

        return View(ViewPath, model);
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
}
