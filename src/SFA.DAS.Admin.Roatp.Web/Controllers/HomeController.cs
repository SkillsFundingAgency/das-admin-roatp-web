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
        string searchUrl = Url.RouteUrl(RouteNames.SelectProvider)!;

        string addProviderUrl = Url.RouteUrl(RouteNames.AddProvider)!;
        return View(new ManageTrainingProviderViewModel { SearchForTrainingProviderUrl = searchUrl, AddANewTrainingProviderUrl = addProviderUrl });
    }

    [Route("/dashboard", Name = RouteNames.Dashboard)]
    public IActionResult Dashboard()
    {
        return Redirect(_configuration.Value.AdminServicesBaseUrl + "Dashboard");
    }
}
