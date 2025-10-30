using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers", Name = RouteNames.SelectProvider)]
public class SelectTrainingProviderController(IValidator<SelectTrainingProviderViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        SelectTrainingProviderViewModel model = new SelectTrainingProviderViewModel();

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(SelectTrainingProviderViewModel model)
    {
        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            var viewModel = new SelectTrainingProviderViewModel();
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(viewModel);
        }

        return RedirectToRoute(RouteNames.ProviderSummary, new { model.Ukprn });
    }
}
