using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
using SFA.DAS.Admin.Roatp.Web.Controllers.ManageRestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageUnrestrictedProvider;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("providers/{ukprn}/restrict/confirm", Name = RouteNames.ConfirmRestrictProvider)]
public class ConfirmRestrictProviderController(
    ISessionService sessionService,
    IOuterApiClient outerApiClient) : Controller
{
    public const string ViewPath = "~/Views/ManageUnrestrictedProvider/ConfirmRestrictProvider/Index.cshtml";
    public const string SuccessBannerMessage = "This training provider can now only deliver approved courses";

    [HttpGet]
    public async Task<IActionResult> Index(int ukprn, CancellationToken cancellationToken)
    {
        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var viewModel = new ConfirmRestrictProviderViewModel
        {
            Ukprn = ukprn,
            ProviderName = providerName,
            CancelUrl = Url.RouteUrl(RouteNames.ProviderSummary, new { ukprn })!
        };

        return View(ViewPath, viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> IndexPost(int ukprn, CancellationToken cancellationToken)
    {
        var providerName = await sessionService.GetProviderName(outerApiClient, ukprn, cancellationToken);
        if (providerName is null)
        {
            return NotFound();
        }

        var response = await outerApiClient.RestrictProvider(
            ukprn,
            CourseType.Apprenticeship,
            new RestrictProviderRequest
            {
                UserId = User.UserId(),
                UserDisplayName = User.UserDisplayName()
            },
            cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

        TempData[ProviderAllowedCoursesController.SuccessBannerTempDataKey] = SuccessBannerMessage;

        return RedirectToRoute(
            RouteNames.ProviderAllowedCourses,
            new { ukprn, routeLearningType = RouteLearningType.Apprenticeship });
    }
}
