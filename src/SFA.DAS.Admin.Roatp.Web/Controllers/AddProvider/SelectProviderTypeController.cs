using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Constants;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/provider-type", Name = RouteNames.SelectProviderType)]
public class SelectProviderTypeController(ISessionService _sessionService, IValidator<SelectProviderTypeSubmitModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        if (sessionModel == null) return RedirectToRoute(RouteNames.AddProvider);

        var model = new SelectProviderTypeViewModel
        {
            ProviderTypes = BuildProviderTypes(sessionModel.ProviderTypeId ?? 0),
            SelectedProviderTypeId = sessionModel.ProviderTypeId ?? 0
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(SelectProviderTypeSubmitModel submitModel)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new SelectProviderTypeViewModel
            {
                ProviderTypes = BuildProviderTypes(submitModel.SelectedProviderTypeId ?? 0),
            };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        sessionModel.ProviderTypeId = submitModel.SelectedProviderTypeId;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        if (sessionModel.OfferApprenticeships != null)
        {
            sessionModel.ClearSessionProperty(nameof(sessionModel.OfferApprenticeships));

            _sessionService.Set(SessionKeys.AddProvider, sessionModel);
        }

        if (submitModel.SelectedProviderTypeId == (int)ProviderType.Supporting)
        {
            return RedirectToRoute(RouteNames.SelectProviderType);
        }

        return RedirectToRoute(RouteNames.SelectOfferApprenticeships);
    }

    private static List<AddProviderTypeSelectionModel> BuildProviderTypes(int providerTypeId)
    {
        return new List<AddProviderTypeSelectionModel>
        {
            new() { Description = ProviderTypeDescription.Main, Id = (int)ProviderType.Main, IsSelected = providerTypeId == (int)ProviderType.Main },
            new() { Description = ProviderTypeDescription.Employer, Id = (int)ProviderType.Employer, IsSelected = providerTypeId == (int)ProviderType.Employer },
            new() { Description = ProviderTypeDescription.Supporting, Id = (int)ProviderType.Supporting, IsSelected = providerTypeId == (int)ProviderType.Supporting },
        };
    }
}
