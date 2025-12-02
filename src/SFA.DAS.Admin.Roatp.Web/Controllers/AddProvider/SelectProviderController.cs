using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new")]
public class SelectProviderController(IValidator<SelectProviderSubmitModel> _validator, ISessionService _sessionService, IOuterApiClient _outerApiClient) : Controller
{
    public const string ExistingUkprnValidationMessage = "This is an existing UKPRN for";

    [HttpGet]
    [Route("select", Name = RouteNames.AddProvider)]
    public IActionResult Index()
    {
        _sessionService.Delete(SessionKeys.AddProvider);

        SelectProviderViewModel model = new();

        return View(model);
    }

    [HttpPost]
    [Route("select", Name = RouteNames.AddProvider)]
    public async Task<IActionResult> Index(SelectProviderSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new SelectProviderViewModel() { Ukprn = submitModel.Ukprn };

            ModelState.AddValidationErrors(result.Errors);

            return View(viewModel);
        }

        ApiResponse<GetOrganisationResponse> organisationApiResponse = await _outerApiClient.GetOrganisation(int.Parse(submitModel.Ukprn!), cancellationToken);

        if (organisationApiResponse.StatusCode == HttpStatusCode.OK)
        {
            GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

            var viewModel = new SelectProviderViewModel() { Ukprn = submitModel.Ukprn };

            ModelState.AddModelError(nameof(SelectProviderSubmitModel.Ukprn), $"{ExistingUkprnValidationMessage} '{organisationResponse.LegalName}'");

            return View(viewModel);
        }

        if (organisationApiResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Unexpected status code returned from GetOrganisation: {organisationApiResponse.StatusCode}");
        }

        ApiResponse<GetUkrlpResponse> ukrlpApiResponse = await _outerApiClient.GetUkrlp(int.Parse(submitModel.Ukprn!)!, cancellationToken);

        if (ukrlpApiResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return RedirectToRoute(RouteNames.ProviderNotFoundInUkrlp);
        }

        if (ukrlpApiResponse.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Unexpected status code returned from GetUkrlp: {ukrlpApiResponse.StatusCode}");
        }

        GetUkrlpResponse ukrlpResponse = ukrlpApiResponse.Content!;

        var sessionModel = new AddProviderSessionModel()
        {
            Ukprn = int.Parse(submitModel.Ukprn!),
            LegalName = ukrlpResponse.LegalName!,
            TradingName = ukrlpResponse.TradingName,
            CompanyNumber = ukrlpResponse.CompanyNumber,
            CharityNumber = ukrlpResponse.CharityNumber
        };

        _sessionService.Set<AddProviderSessionModel>(SessionKeys.AddProvider, sessionModel);

        return RedirectToRoute(RouteNames.ProviderDetails);
    }

    [HttpGet]
    [Route("not-found", Name = RouteNames.ProviderNotFoundInUkrlp)]
    public IActionResult ProviderNotFoundInUkrlp()
    {
        var viewModel = new ProviderNotFoundInUkrlpViewModel() { AddANewTrainingProviderUrl = Url.RouteUrl(RouteNames.AddProvider)! };

        return View(viewModel);
    }
}