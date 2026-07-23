using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class RestrictedCourseItemViewModel
{
    public string LarsCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Level { get; set; }
    public LearningType LearningType { get; set; }

    public string DisplayTitle => $"{Title} (Level {Level})";
    public string LearningTypeDescription => LearningType.GetDescription();
    public string LearningTypeTagClass => LearningType.GetTagClass();
}
