using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("[Controller]")]
public class ErrorController(IOptions<ApplicationConfiguration> _applicationConfiguration, ILogger<ErrorController> _logger) : Controller
{
    public const string PageNotFoundViewName = "PageNotFound";
    public const string ServiceErrorViewName = "ServiceError";
    public const string AccessDeniedViewName = "AccessDenied";

    [Route("{status}")]
    [HttpGet]
    public IActionResult Index([FromRoute] int status, [FromQuery] string returnUrl)
    {
        return status switch
        {
            StatusCodes.Status403Forbidden => Handle403(),
            StatusCodes.Status404NotFound => View(PageNotFoundViewName),
            _ => View(ServiceErrorViewName)
        };
    }

    private IActionResult Handle403()
    {
        if (HttpContext.User != null)
        {
            var userName = HttpContext.User.Identity?.Name ?? HttpContext.User.FindFirstValue(ClaimTypes.Upn);
            var roles = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == Roles.RoleClaimType).Select(c => c.Value);

            _logger.LogError("AccessDenied - User '{UserName}' does not have a valid role. They have the following roles: '{Roles}'", userName, string.Join(",", roles));
        }
        var model = new AccessDeniedViewModel(_applicationConfiguration.Value.DfESignInServiceHelpUrl);
        return View(AccessDeniedViewName, model);
    }
}
