using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/details", Name = RouteNames.ProviderDetails)]
public class ProviderDetailsController(ISessionService _sessionService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel == null) return RedirectToRoute(RouteNames.AddProvider);

        var model = new ProviderDetailsViewModel()
        {
            Ukprn = sessionModel.Ukprn,
            LegalName = sessionModel.LegalName ?? "Not applicable",
            TradingName = sessionModel.TradingName ?? "Not applicable",
            CompanyNumber = sessionModel.CompanyNumber ?? "Not applicable",
            CharityNumber = sessionModel.CharityNumber ?? "Not applicable",
            AddProviderRouteUrl = Url.RouteUrl(RouteNames.ProviderDetails)!,
            SelectProviderUrl = Url.RouteUrl(RouteNames.AddProvider)!
        };

        return View(model);
    }
}
