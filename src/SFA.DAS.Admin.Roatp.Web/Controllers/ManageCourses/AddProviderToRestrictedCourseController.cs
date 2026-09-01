using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/{larsCode}/providers")]
public class AddProviderToRestrictedCourseController(
    IOuterApiClient outerApiClient,
    IValidator<AddProviderToRestrictedCourseSubmitModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/AddProviderToRestrictedCourse/Index.cshtml";

    [HttpGet("add", Name = RouteNames.AddProviderToRestrictedCourse)]
    public async Task<IActionResult> Index([FromRoute] string larsCode, CancellationToken cancellationToken)
    {
        var courseDetails = await GetRestrictedCourseAsync(larsCode, cancellationToken);
        if (courseDetails is null)
        {
            return NotFound();
        }

        return View(ViewPath, BuildViewModel(larsCode, courseDetails));
    }

    [HttpPost("add")]
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
            ModelState.Clear();
            ModelState.AddValidationErrors(validationResult.Errors);
            return View(ViewPath, BuildViewModel(larsCode, courseDetails));
        }

        return RedirectToRoute(RouteNames.AddProviderToRestrictedCourse, new { larsCode });
    }

    private async Task<GetRestrictedCourseDetailsResponse?> GetRestrictedCourseAsync(
        string larsCode,
        CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetNotAllowedProvidersForCourse(larsCode, cancellationToken);
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
