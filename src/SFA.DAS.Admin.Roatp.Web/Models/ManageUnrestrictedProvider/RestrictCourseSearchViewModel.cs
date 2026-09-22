using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class RestrictCourseSearchViewModel : RestrictCourseSearchSubmitModel, IBackLink
{
    public int Ukprn { get; set; }
    public IEnumerable<SelectListItem> Courses { get; set; } = [];
}
