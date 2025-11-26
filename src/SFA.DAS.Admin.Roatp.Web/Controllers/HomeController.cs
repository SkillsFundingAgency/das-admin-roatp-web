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
        string addProviderUrl = _configuration.Value.AdminServicesBaseUrl + "organisations-ukprn";
        string allowedListUrl = _configuration.Value.AdminServicesBaseUrl + "Roatp/AllowedProviders";

        return View(new ManageTrainingProviderViewModel { SearchForTrainingProviderUrl = searchUrl, AddANewTrainingProviderUrl = addProviderUrl, AddUkprnToAllowListUrl = allowedListUrl });
    }

    [Route("/dashboard", Name = RouteNames.Dashboard)]
    public IActionResult Dashboard()
    {
        return Redirect(_configuration.Value.AdminServicesBaseUrl + "Dashboard");
    }
}
