using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("selectProvider", Name = RouteNames.SelectProvider)]
public class SelectTrainingProviderController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<SelectTrainingProviderSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        _sessionService.Delete(SessionKeys.EditOrganisation);
        SelectTrainingProviderViewModel model = new SelectTrainingProviderViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(SelectTrainingProviderSubmitViewModel submitModel, CancellationToken cancellationToken)
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

        var organisationResponse = await _outerApiClient.GetOrganisation(submitModel.Ukprn!, cancellationToken);

        if (organisationResponse == null) return RedirectToRoute(RouteNames.Home);

        var organisationSessionModel = (EditOrganisationSessionModel)organisationResponse;
        _sessionService.Set<EditOrganisationSessionModel>(SessionKeys.EditOrganisation, organisationSessionModel);

        return RedirectToRoute(RouteNames.ProviderSummary);
    }
}
