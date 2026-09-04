using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;
using SFA.DAS.Admin.Roatp.Web.Models.Session;
using SFA.DAS.Admin.Roatp.Web.Services;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers/add", Name = RouteNames.AddProviderToRestrictedCourse)]
public class AddProviderToRestrictedCourseController(
    IOuterApiClient outerApiClient,
    ISessionService sessionService,
    IValidator<AddProviderToRestrictedCourseSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/AddProviderToRestrictedCourse/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        sessionService.Delete(SessionKeys.AddProviderToRestrictedCourse);

        var courseDetails = await GetRestrictedCourseAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(larsCode, courseDetails));
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        [FromRoute] string larsCode,
        AddProviderToRestrictedCourseSubmitModel submitModel,
        CancellationToken cancellationToken)
    {
        var courseDetails = await GetRestrictedCourseAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        var validationResult = validator.Validate(submitModel);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, BuildViewModel(larsCode, courseDetails));
        }

        var provider = courseDetails.Providers.FirstOrDefault(p => p.Ukprn.ToString() == submitModel.SelectedUkprn);
        if (provider is null)
        {
            return NotFound();
        }
        var viewModel = BuildViewModel(larsCode, courseDetails);
        sessionService.Set(SessionKeys.AddProviderToRestrictedCourse, new AddProviderToRestrictedCourseSessionModel
        {
            LarsCode = larsCode,
            CourseDisplayTitle = viewModel.DisplayTitle,
            Ukprn = provider.Ukprn,
            LegalName = provider.ProviderName
        });

        return RedirectToRoute(RouteNames.ConfirmAddProviderToRestrictedCourse, new { larsCode });
    }

    private async Task<GetRestrictedCourseDetailsResponse?> GetRestrictedCourseAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetProvidersRestrictedForCourse(larsCode, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync();

        var courseDetails = response.Content;
        if (courseDetails is null || !courseDetails.IsCourseRestricted)
        {
            return null;
        }

        return courseDetails;
    }

    private static AddProviderToRestrictedCourseViewModel BuildViewModel(
        string larsCode,
        GetRestrictedCourseDetailsResponse courseDetails)
        => new()
        {
            LarsCode = larsCode,
            Title = courseDetails.CourseName,
            Level = courseDetails.Level,
            Providers = courseDetails.Providers
                .OrderBy(provider => provider.ProviderName)
                .Select(provider => new SelectListItem(
                    $"{provider.ProviderName} UKPRN: {provider.Ukprn}",
                    provider.Ukprn.ToString()))
        };
}
