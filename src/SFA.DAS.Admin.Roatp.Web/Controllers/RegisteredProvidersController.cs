using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("[Controller]")]
public class RegisteredProvidersController(IOrganisationsService _organisationsService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string query, CancellationToken cancellationToken)
    {
        query ??= string.Empty;
        var searchTerm = query.Trim();
        if (searchTerm.Length < 3) return Ok(new List<OrganisationModel>());

        var organisations = await _organisationsService.GetOrganisations(cancellationToken);

        var matchedOrganisations = organisations
            .Where(provider => provider.LegalName.Contains(query, StringComparison.OrdinalIgnoreCase)
                               || provider.Ukprn.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.LegalName);

        return Ok(matchedOrganisations.Take(100));
    }
}
