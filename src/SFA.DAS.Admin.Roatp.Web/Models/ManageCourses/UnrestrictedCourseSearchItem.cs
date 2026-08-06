using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class UnrestrictedCourseSearchItem : ICourseDisplayModel
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public string DisplayTitle => this.GetDisplayTitle();

    public static implicit operator UnrestrictedCourseSearchItem(RestrictedCourseModel course) => new()
    {
        LarsCode = course.LarsCode,
        Title = course.Title,
        Level = course.Level
    };
}
