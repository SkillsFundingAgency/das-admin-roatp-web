using SFA.DAS.Admin.Roatp.Web.Models.CourseRestrictions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class ProviderRestrictedCourseSetLastStartDateViewModel : SetLastDateStartsSubmitModel
{
    public int Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public required string CancelUrl { get; set; }
}
