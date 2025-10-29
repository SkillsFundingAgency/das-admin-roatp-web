using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers", Name = RouteNames.SelectProvider)]
public class SelectTrainingProviderController(ISessionService _sessionService, IValidator<SelectTrainingProviderSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        _sessionService.Delete(SessionKeys.EditOrganisation);
        SelectTrainingProviderViewModel model = new SelectTrainingProviderViewModel();

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(SelectTrainingProviderSubmitViewModel submitModel)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = new SelectTrainingProviderViewModel();
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(model);
        }

        return RedirectToRoute(RouteNames.ProviderSummary, new { submitModel.Ukprn });
    }
}
