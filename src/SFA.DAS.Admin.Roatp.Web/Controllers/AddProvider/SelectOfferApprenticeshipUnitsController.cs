using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Filters;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/offer-apprenticeship-units", Name = RouteNames.SelectOfferApprenticeshipUnits)]
[RequiresSessionModel<AddProviderSessionModel>(SessionKeys.AddProvider, RouteNames.Home)]
public class SelectOfferApprenticeshipUnitsController(ISessionService _sessionService, IValidator<OfferApprenticeshipUnitsSubmitModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel!.ProviderTypeId == (int)ProviderType.Supporting) return RedirectToRoute(RouteNames.Home);

        var viewModel = new OfferApprenticeshipUnitsViewModel
        {
            ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(sessionModel.OffersApprenticeshipUnits),
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Index(OfferApprenticeshipUnitsSubmitModel submitModel)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new OfferApprenticeshipUnitsViewModel
            {
                IsApprenticeshipUnitsOffered = submitModel.IsApprenticeshipUnitsOffered,
                ApprenticeshipUnitsSelection = BuildApprenticeshipTypesChoices(submitModel.IsApprenticeshipUnitsOffered)
            };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        sessionModel!.OffersApprenticeshipUnits = submitModel.IsApprenticeshipUnitsOffered;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        if (sessionModel.RedirectedFromSummaryPage)
        {
            return RedirectToRoute(RouteNames.ProviderDetailsSummary);
        }

        return RedirectToRoute(RouteNames.SelectOrganisationType);
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
