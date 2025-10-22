using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("[Controller]", Name = RouteNames.RegisteredProviders)]
public class RegisteredProvidersController(IOuterApiClient _outerApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string query, CancellationToken cancellationToken)
    {
        query ??= string.Empty;
        var searchTerm = query.Trim();
        if (searchTerm.Length < 3) return Ok(new List<OrganisationModel>());

        var organisationsResponse = await _outerApiClient.GetOrganisations(cancellationToken);
        var providers = organisationsResponse.Organisations;

        var matchedProviders = providers
            .Where(provider => provider.LegalName.Contains(query, StringComparison.OrdinalIgnoreCase)
                               || provider.Ukprn.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.LegalName);

        return Ok(matchedProviders.Take(100));
    }
}
