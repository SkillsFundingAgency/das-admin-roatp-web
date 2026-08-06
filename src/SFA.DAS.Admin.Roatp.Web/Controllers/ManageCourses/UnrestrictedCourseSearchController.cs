using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

namespace SFA.DAS.Admin.Roatp.Web.Controllers.ManageCourses;

[Authorize(Roles = Roles.RoatpAdminTeam)]
[Route("unrestricted-courses/search", Name = RouteNames.UnrestrictedCourseSearch)]
public class UnrestrictedCourseSearchController(IValidator<UnrestrictedCourseSearchViewModel> validator) : Controller
{
    public const string ViewPath = "~/Views/ManageCourses/UnrestrictedCourseSearch/Index.cshtml";

    [HttpGet]
    public IActionResult Index()
    {
        return View(ViewPath, new UnrestrictedCourseSearchViewModel());
    }

    [HttpPost]
    public IActionResult Index(UnrestrictedCourseSearchViewModel model)
    {
        var result = validator.Validate(model);

        if (!result.IsValid)
        {
            ModelState.AddValidationErrors(result.Errors);
            return View(ViewPath, new UnrestrictedCourseSearchViewModel());
        }

        return RedirectToRoute(RouteNames.UnrestrictedCourseSearch);
    }
}
