using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("selectProvider", Name = RouteNames.SelectProvider)]
public class TrainingProviderController(ISessionService _sessionService, IValidator<SelectTrainingProviderSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        SelectTrainingProviderViewModel model = new SelectTrainingProviderViewModel();

        // THIS IS A TEMPORARY MEASURE TO ENABLE TESTING, REMOVED in CSP-2210
        var savedOrganisation = _sessionService.Get<OrganisationSessionModel>(SessionKeys.EditOrganisation);

        if (savedOrganisation != null)
        {
            model.MatchedResult = savedOrganisation.Ukprn + ": " + savedOrganisation.LegalName;

        }
        // -- end of temporary measure

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(SelectTrainingProviderSubmitViewModel submitModel, CancellationToken cancellationToken)
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

        var organisationSessionModel = new OrganisationSessionModel { LegalName = submitModel.LegalName!, Ukprn = submitModel.Ukprn! };

        _sessionService.Set<OrganisationSessionModel>(SessionKeys.EditOrganisation, organisationSessionModel);

        return RedirectToRoute(RouteNames.SelectProvider);
    }
}
