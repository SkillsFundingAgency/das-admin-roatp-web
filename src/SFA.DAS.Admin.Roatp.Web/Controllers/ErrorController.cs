using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("[Controller]")]
public class ErrorController(IOptions<ApplicationConfiguration> _applicationConfiguration, ILogger<ErrorController> _logger) : Controller
{
    [Route("{status}")]
    public IActionResult Index([FromRoute] int status, [FromQuery] string returnUrl)
    {
        Func<IActionResult> func403 = () =>
        {
            if (HttpContext.User != null)
            {
                var userName = HttpContext.User.Identity?.Name ?? HttpContext.User.FindFirstValue(ClaimTypes.Upn);
                var roles = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == Roles.RoleClaimType).Select(c => c.Value);

                _logger.LogError("AccessDenied - User '{UserName}' does not have a valid role. They have the following roles: '{Roles}'", userName, string.Join(",", roles));
            }
            var model = new AccessDeniedViewModel(_applicationConfiguration.Value.DfESignInServiceHelpUrl);
            return View("~/Views/Error/AccessDenied.cshtml", model);
        };


        return status switch
        {
            403 => func403(),
            _ => View("~/Views/Error/ServiceError.cshtml")
        };
    }
}
