using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("restricted-courses/search", Name = RouteNames.SearchCourseToRestrict)]
public class SearchCourseToRestrictController(IValidator<SearchCourseToRestrictViewModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/SearchCourseToRestrict/Index.cshtml";

    [HttpGet]
    public IActionResult Index()
    {
        return View(ViewPath, new SearchCourseToRestrictViewModel());
    }

    [HttpPost]
    public IActionResult Index(SearchCourseToRestrictViewModel model)
    {
        var result = validator.Validate(model);

        if (!result.IsValid)
        {
            ModelState.AddValidationErrors(result.Errors);
            return View(ViewPath, new SearchCourseToRestrictViewModel());
        }

        return RedirectToRoute(RouteNames.SearchCourseToRestrict);
    }
}
