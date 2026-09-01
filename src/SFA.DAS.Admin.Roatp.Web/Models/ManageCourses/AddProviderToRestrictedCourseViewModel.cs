using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class AddProviderToRestrictedCourseViewModel : AddProviderToRestrictedCourseSubmitModel, ICourseDisplayModel, IBackLink
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public string DisplayTitle => this.GetDisplayTitle();
    public IEnumerable<SelectListItem> Providers { get; set; } = [];
}
