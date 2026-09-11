using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

public class UnrestrictedCourseSearchViewModel : UnrestrictedCourseSearchSubmitModel, IBackLink
{
    public IEnumerable<SelectListItem> Courses { get; set; } = [];
}
