namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class RestrictedCourseModel
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public LearningType LearningType { get; set; }
}
