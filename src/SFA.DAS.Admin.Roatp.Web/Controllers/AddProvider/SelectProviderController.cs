using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Refit;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models;
using System.Net;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.AddProvider;

[Route("providers/new")]
public class SelectProviderController(IValidator<AddProviderSubmitModel> _validator, IOuterApiClient _outerApiClient) : Controller
{
    public const string ExistingUkprnValidationMessage = "This is an existing UKPRN for";

    [HttpGet]
    [Route("select", Name = RouteNames.AddProvider)]
    public IActionResult Index()
    {
        AddProviderViewModel model = new();

        return View(model);
    }

    [HttpPost]
    [Route("select", Name = RouteNames.AddProvider)]
    public async Task<IActionResult> Index(AddProviderSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var viewModel = new AddProviderViewModel() { Ukprn = submitModel.Ukprn };
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View(viewModel);
        }

        ApiResponse<GetOrganisationResponse> organisationApiResponse = await _outerApiClient.GetOrganisation(int.Parse(submitModel.Ukprn!), cancellationToken);

        if (organisationApiResponse.StatusCode == HttpStatusCode.OK)
        {
            GetOrganisationResponse organisationResponse = organisationApiResponse.Content!;

            var viewModel = new AddProviderViewModel() { Ukprn = submitModel.Ukprn };

            ModelState.AddModelError(nameof(AddProviderSubmitModel.Ukprn), $"{ExistingUkprnValidationMessage} '{organisationResponse.LegalName}'");

            return View(viewModel);
        }

        ApiResponse<GetUkrlpResponse> ukrlpResponse = await _outerApiClient.GetUkrlp(int.Parse(submitModel.Ukprn!)!, cancellationToken);

        if (ukrlpResponse.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return RedirectToRoute(RouteNames.AddProvider);
        }

        return RedirectToRoute(RouteNames.ProviderNotFoundInUkrlp);
    }

    [HttpGet]
    [Route("not-found", Name = RouteNames.ProviderNotFoundInUkrlp)]
    public IActionResult ProviderNotFoundInUkrlp()
    {
        var viewModel = new ProviderNotFoundInUkrlpViewModel() { AddANewTrainingProviderUrl = Url.RouteUrl(RouteNames.AddProvider)! };

        return View(viewModel);
    }
}