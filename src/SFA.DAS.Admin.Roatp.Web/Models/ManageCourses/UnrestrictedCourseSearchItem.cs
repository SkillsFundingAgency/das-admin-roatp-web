using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class UnrestrictedCourseSearchItem
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public string SearchTerm { get; set; } = string.Empty;

    public static implicit operator UnrestrictedCourseSearchItem(RestrictedCourseModel course) => new()
    {
        LarsCode = course.LarsCode,
        Title = course.Title,
        Level = course.Level,
        SearchTerm = $"{course.Title} (Level {course.Level})"
    };
}
