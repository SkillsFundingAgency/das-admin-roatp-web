namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class AllowedCourseType
{
    public CourseType CourseType { get; set; }
    public bool? IsRestricted { get; set; }
    public int? RestrictedCount { get; set; }
    public int? AllowedCount { get; set; }
}
