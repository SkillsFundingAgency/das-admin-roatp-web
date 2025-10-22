using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("[Controller]")]
[Route("", Name = RouteNames.Home)]
public class HomeController(IOptions<ApplicationConfiguration> _configuration, ISessionService _sessionService) : Controller
{
    [Authorize(Roles = Roles.RoatpAdminTeam)]
    public IActionResult Index()
    {
        _sessionService.Delete(SessionKeys.EditOrganisation);
        var searchUrl = Url.RouteUrl(RouteNames.SelectProvider);

        return View(new ManageTrainingProviderViewModel { SearchForTrainingProviderUrl = searchUrl! });
    }

    [Route("/dashboard", Name = RouteNames.Dashboard)]
    public IActionResult Dashboard()
    {
        _sessionService.Delete(SessionKeys.EditOrganisation);
        return Redirect(_configuration.Value.AdminServicesBaseUrl + "Dashboard");
    }
}
