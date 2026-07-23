namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class RestrictedCourseModel
{
    public string LarsCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Level { get; set; }
    public LearningType LearningType { get; set; }
}
