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

[Route("providers/new/offer-apprenticeships", Name = RouteNames.SelectOfferApprenticeships)]
[RequiresSessionModel<AddProviderSessionModel>(SessionKeys.AddProvider, RouteNames.Home)]
public class SelectOfferApprenticeshipsController(ISessionService _sessionService, IValidator<OfferApprenticeshipsSubmitModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel!.ProviderTypeId == (int)ProviderType.Supporting) return RedirectToRoute(RouteNames.Home);

        var viewModel = new OfferApprenticeshipsViewModel
        {
            ApprenticeshipsSelection = BuildApprenticeshipsChoices(sessionModel.OffersApprenticeships),
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Index(OfferApprenticeshipsSubmitModel submitModel)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new OfferApprenticeshipsViewModel
            {
                IsApprenticeshipsOffered = submitModel.IsApprenticeshipsOffered,
                ApprenticeshipsSelection = BuildApprenticeshipsChoices(submitModel.IsApprenticeshipsOffered)
            };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        sessionModel!.OffersApprenticeships = submitModel.IsApprenticeshipsOffered;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        return RedirectToRoute(RouteNames.SelectOfferApprenticeshipUnits);
    }

    private static List<ApprenticeshipsSelectionModel> BuildApprenticeshipsChoices(bool? containsApprenticeshipUnits)
    {
        return new List<ApprenticeshipsSelectionModel>
            {
                new() { Description = "Yes", Id = true, IsSelected = containsApprenticeshipUnits is true},
                new() { Description = "No", Id = false, IsSelected =  containsApprenticeshipUnits is false}
            };
    }
}
