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
    private const string ReturnToDashboard = "Return to dashboard";

    [Authorize(Roles = Roles.RoatpAdminTeam)]
    public IActionResult Index()
    {
        _sessionService.Delete(SessionKeys.AddProvider);

        string searchUrl = Url.RouteUrl(RouteNames.SelectProvider)!;
        string addProviderUrl = Url.RouteUrl(RouteNames.AddProvider)!;
        string restrictedCoursesUrl = Url.RouteUrl(RouteNames.RestrictedCourses)!;
        string allowedListUrl = new UriBuilder(_configuration.Value.AdminServicesBaseUrl) { Path = ExternalPaths.AdminServiceAllowedList }.Uri.ToString();
        string dashboardUrl = Url.RouteUrl(RouteNames.Dashboard)!;

        return View(new ManageTrainingProviderViewModel
        {
            SearchForTrainingProviderUrl = searchUrl,
            AddANewTrainingProviderUrl = addProviderUrl,
            AddUkprnToAllowListUrl = allowedListUrl,
            ViewRestrictedCoursesUrl = restrictedCoursesUrl,
            BackLinkUrl = dashboardUrl,
            BackLinkText = ReturnToDashboard
        });
    }

    [Route("/dashboard", Name = RouteNames.Dashboard)]
    public IActionResult Dashboard()
    {
        return Redirect(_configuration.Value.AdminServicesBaseUrl + "Dashboard");
    }
}
