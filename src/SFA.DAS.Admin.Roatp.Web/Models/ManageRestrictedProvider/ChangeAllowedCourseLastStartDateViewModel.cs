using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageRestrictedProvider;

public class ChangeAllowedCourseLastStartDateViewModel : ChangeAllowedCourseLastStartDateSubmitModel
{
    public int Ukprn { get; set; }
    public RouteLearningType RouteLearningType { get; set; }
    public required string LarsCode { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public DateTime LastDateStarts { get; set; }
    public required string CancelUrl { get; set; }

    public string LastDateStartsText => LastDateStarts.ToDisplayString();
}
