namespace SFA.DAS.Admin.Roatp.Domain.Models;
public class AllowedCourseType
{
    public int CourseTypeId { get; set; }
    public string CourseTypeName { get; set; } = string.Empty;
    public LearningType LearningType { get; set; }
}
