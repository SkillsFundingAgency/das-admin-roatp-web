using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class CourseDisplayModelExtensions
{
    public static string GetDisplayTitle(this ICourseDisplayModel course) =>
        GetDisplayTitle(course.Title, course.Level);

    public static string GetDisplayTitle(string courseName, int level) =>
        $"{courseName} (Level {level})";
}
