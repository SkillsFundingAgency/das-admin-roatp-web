using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/providertype", Name = RouteNames.SelectProviderType)]
public class SelectProviderTypeController(ISessionService _sessionService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel == null) return RedirectToRoute(RouteNames.AddProvider);

        var model = new SelectProviderTypeViewModel
        {
            ProviderTypes = BuildProviderTypes(sessionModel.ProviderTypeId ?? 0),
            ProviderTypeId = sessionModel.ProviderTypeId ?? 0
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(SelectProviderTypeSubmitModel submitModel)
    {
        var viewModel = new SelectProviderTypeViewModel
        {
            ProviderTypes = BuildProviderTypes(submitModel.ProviderTypeId),
            ProviderTypeId = submitModel.ProviderTypeId
        };

        return View(viewModel);
    }

    private static List<AddProviderTypeSelectionModel> BuildProviderTypes(int providerTypeId)
    {
        return new List<AddProviderTypeSelectionModel>
        {
            new() { Description = "Main provider", Id = (int)ProviderType.Main, IsSelected = providerTypeId == (int)ProviderType.Main },
            new() { Description = "Employer provider", Id = (int)ProviderType.Employer, IsSelected = providerTypeId == (int)ProviderType.Employer },
            new() { Description = "Supporting provider", Id = (int)ProviderType.Supporting, IsSelected = providerTypeId == (int)ProviderType.Supporting },
        };
    }
}
