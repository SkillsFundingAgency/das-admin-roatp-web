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

[Route("providers/new/organisation-type", Name = RouteNames.SelectOrganisationType)]
[RequiresSessionModel<AddProviderSessionModel>(SessionKeys.AddProvider, RouteNames.Home)]
public class SelectOrganisationTypeController(ISessionService _sessionService, IValidator<OrganisationTypeSubmitModel> _validator, IOrganisationTypesService _organisationTypesService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider)!;

        var organisationTypesResponse = await _organisationTypesService.GetOrganisationTypes(cancellationToken);

        List<OrganisationTypeModel> organisationTypes = new List<OrganisationTypeModel>();

        foreach (var organisationType in organisationTypesResponse)
        {
            organisationTypes.Add(new OrganisationTypeModel { Id = organisationType.Id, Description = organisationType.Description, IsSelected = organisationType.Id == sessionModel.OrganisationTypeId });
        }

        var viewModel = new OrganisationTypeViewModel
        {
            LegalName = sessionModel.LegalName,
            OrganisationTypes = organisationTypes.OrderBy(r => r.Id).ToList(),
            OrganisationTypeId = sessionModel.OrganisationTypeId
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(OrganisationTypeSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<AddProviderSessionModel>(SessionKeys.AddProvider);

        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var organisationTypesResponse = await _organisationTypesService.GetOrganisationTypes(cancellationToken);

            List<OrganisationTypeModel> organisationTypes = new List<OrganisationTypeModel>();

            foreach (var organisationType in organisationTypesResponse)
            {
                organisationTypes.Add(new OrganisationTypeModel { Id = organisationType.Id, Description = organisationType.Description, IsSelected = organisationType.Id == sessionModel!.OrganisationTypeId });
            }

            var viewModel = new OrganisationTypeViewModel
            {
                LegalName = sessionModel!.LegalName,
                OrganisationTypes = organisationTypes.OrderBy(r => r.Id).ToList(),
                OrganisationTypeId = sessionModel.OrganisationTypeId
            };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        sessionModel!.OrganisationTypeId = submitModel.OrganisationTypeId;

        _sessionService.Set(SessionKeys.AddProvider, sessionModel);

        return RedirectToRoute(RouteNames.SelectOrganisationType);
    }
}
