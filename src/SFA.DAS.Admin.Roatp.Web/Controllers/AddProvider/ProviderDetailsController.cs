using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/details", Name = RouteNames.ProviderDetails)]
public class ProviderDetailsController(ISessionService _sessionService) : Controller
{
    public const string NotApplicableValue = "Not applicable";

    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel == null) return RedirectToRoute(RouteNames.AddProvider);

        var model = new ProviderDetailsViewModel()
        {
            Ukprn = sessionModel.Ukprn,
            LegalName = sessionModel.LegalName,
            TradingName = sessionModel.TradingName ?? NotApplicableValue,
            CompanyNumber = sessionModel.CompanyNumber ?? NotApplicableValue,
            CharityNumber = sessionModel.CharityNumber ?? NotApplicableValue,
            AddProviderRouteUrl = Url.RouteUrl(RouteNames.ProviderDetails)!,
            SelectProviderUrl = Url.RouteUrl(RouteNames.AddProvider)!
        };

        return View(model);
    }
}
