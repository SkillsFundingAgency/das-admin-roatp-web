using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/offer-apprenticeship-units", Name = RouteNames.SelectOfferApprenticeshipUnits)]
public class SelectOfferApprenticeshipUnitsController(ISessionService _sessionService, IValidator<OfferApprenticeshipUnitsSubmitModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel == null) return RedirectToRoute(RouteNames.Home);

        if (sessionModel.ProviderTypeId == (int)ProviderType.Supporting) return RedirectToRoute(RouteNames.SelectProviderType);

        var viewModel = new OfferApprenticeshipUnitsViewModel
        {
            ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(sessionModel.OfferApprenticeshipUnits),
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Index(OfferApprenticeshipUnitsSubmitModel submitModel)
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel == null) return RedirectToRoute(RouteNames.Home);

        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new OfferApprenticeshipUnitsViewModel
            {
                ApprenticeshipUnitsSelectionId = submitModel.ApprenticeshipUnitsSelectionId,
                ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(submitModel.ApprenticeshipUnitsSelectionId)
            };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        sessionModel.OfferApprenticeshipUnits = submitModel.ApprenticeshipUnitsSelectionId;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        if (submitModel.ApprenticeshipUnitsSelectionId == false)
        {
            return RedirectToRoute(RouteNames.SelectOfferApprenticeshipUnits);
        }

        return RedirectToRoute(RouteNames.SelectOfferApprenticeshipUnits);
    }

    private static List<ApprenticeshipUnitsSelectionModel> BuildApprenticeshipTypesChoices(bool? selectedId)
    {
        return new List<ApprenticeshipUnitsSelectionModel>
        {
            new() { Description = "Yes", Id = true, IsSelected = selectedId is true},
            new() { Description = "No", Id = false, IsSelected = selectedId is false},
        };
    }
}
