using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class ChangeRestrictedCourseLastStartDateViewModel : SetLastDateStartsSubmitModel, IBackLink
{
    public int Ukprn { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public required string CancelUrl { get; set; }
}
