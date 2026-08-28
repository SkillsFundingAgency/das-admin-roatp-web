using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/search", Name = RouteNames.SearchProvidersForRestrictedCourse)]
public class SearchProvidersForRestrictedCourseController(
    LarsCodeValidator larsCodeValidator,
    INotAllowedProvidersService notAllowedProvidersService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var validationResult = await larsCodeValidator.ValidateAsync(
            new LarsCodeModel { LarsCode = larsCode },
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return NotFound();
        }

        var searchTerm = (query ?? string.Empty).Trim();
        if (searchTerm.Length < 1)
        {
            return Ok(Array.Empty<OrganisationModel>());
        }

        var courseDetails = await notAllowedProvidersService.GetNotAllowedProvidersAsync(larsCode, cancellationToken);
        if (courseDetails is null || !courseDetails.IsCourseRestricted)
        {
            return NotFound();
        }

        return Ok(SearchProviders(courseDetails.Providers, searchTerm));
    }

    private static IEnumerable<OrganisationModel> SearchProviders(
        IEnumerable<ProviderCourseModel> providers,
        string searchTerm)
        => providers
            .Where(provider =>
                provider.ProviderName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || provider.Ukprn.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(provider => provider.ProviderName)
            .Select(provider => new OrganisationModel
            {
                Ukprn = provider.Ukprn,
                LegalName = provider.ProviderName
            });
}
