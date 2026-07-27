using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class CourseDisplayModelExtensions
{
    public static string GetDisplayTitle(this ICourseDisplayModel course) => $"{course.Title} (Level {course.Level})";
}
