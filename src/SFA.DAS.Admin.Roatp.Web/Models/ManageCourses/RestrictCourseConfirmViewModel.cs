namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class RestrictCourseConfirmViewModel
{
    public required string LarsCode { get; set; }
    public required string DisplayName { get; set; }
    public string CancelUrl { get; set; } = "#";
}
