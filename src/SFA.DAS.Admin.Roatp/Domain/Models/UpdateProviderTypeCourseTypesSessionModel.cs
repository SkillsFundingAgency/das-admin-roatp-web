namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class UpdateProviderTypeCourseTypesSessionModel
{
    public ProviderType ProviderType { get; set; } = ProviderType.Supporting;
    public List<int> CourseTypeIds { get; set; } = [];
}