using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class RestrictedCourseItemViewModel : ICourseDisplayModel
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public LearningType LearningType { get; set; }

    public string DisplayTitle => this.GetDisplayTitle();
    public string LearningTypeDescription => LearningType.GetDescription();
    public string LearningTypeTagClass => LearningType.GetTagClass();

    public static implicit operator RestrictedCourseItemViewModel(RestrictedCourseModel course) => new()
    {
        LarsCode = course.LarsCode,
        Title = course.Title,
        Level = course.Level,
        LearningType = course.LearningType
    };
}
