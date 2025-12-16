using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Filters;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new/provider-details-summary", Name = RouteNames.ProviderDetailsSummary)]
[RequiresSessionModel<AddProviderSessionModel>(SessionKeys.AddProvider, RouteNames.Home)]
public class ProviderDetailsSummaryController(ISessionService _sessionService, IValidator<ProviderDetailsSummaryViewModel> _validator, IPostOrganisationService _postOrganisationService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider)!;

        sessionModel.RedirectedFromSummaryPage = true;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        ProviderDetailsSummaryViewModel viewModel = sessionModel;

        var result = _validator.Validate(viewModel);

        if (!result.IsValid)
        {
            ModelState.AddValidationErrors(result.Errors);
        }

        viewModel.ManageProviderLink = Url.RouteUrl(RouteNames.Home)!;
        viewModel.ProviderTypeChangeLink = Url.RouteUrl(RouteNames.SelectProviderType)!;
        viewModel.OffersApprenticeshipsChangeLink = Url.RouteUrl(RouteNames.SelectOfferApprenticeships)!;
        viewModel.OffersApprenticeshipUnitsChangeLink = Url.RouteUrl(RouteNames.SelectOfferApprenticeshipUnits)!;
        viewModel.OrganisationTypeChangeLink = Url.RouteUrl(RouteNames.SelectOrganisationType)!;

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProviderDetailsSummaryViewModel viewModel, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(viewModel);

        if (!result.IsValid)
        {
            return RedirectToRoute(RouteNames.ProviderDetailsSummary);
        }

        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider)!;

        await _postOrganisationService.PostOrganisation(sessionModel, cancellationToken);

        sessionModel.OrganisationSubmitted = true;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        return RedirectToRoute(RouteNames.AddProviderConfirmation);
    }

    [HttpGet]
    [Route("providers/new/provider-added", Name = RouteNames.AddProviderConfirmation)]
    public IActionResult AddProviderConfirmation()
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider)!;

        if (!sessionModel.OrganisationSubmitted)
        {
            return RedirectToRoute(RouteNames.Home);
        }

        var viewModel = new AddProviderConfirmationViewModel()
        {
            LegalName = sessionModel.LegalName,
            DashboardLink = Url.RouteUrl(RouteNames.Home)!
        };

        return View(viewModel);
    }
}
