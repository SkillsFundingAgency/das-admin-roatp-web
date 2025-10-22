using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Route("[Controller]")]
[Route("")]
public class HomeController(IOptions<ApplicationConfiguration> _configuration) : Controller
{
    [Authorize(Roles = Roles.RoatpAdminTeam)]
    public IActionResult Index()
    {

        var searchUrl = Url.RouteUrl(RouteNames.SelectProvider);

        return View(new ManageTrainingProviderViewModel { SearchForTrainingProviderUrl = searchUrl! });
    }

    [Route("/dashboard", Name = RouteNames.Dashboard)]
    public IActionResult Dashboard()
    {
        return Redirect(_configuration.Value.AdminServicesBaseUrl + "Dashboard");
    }
}
