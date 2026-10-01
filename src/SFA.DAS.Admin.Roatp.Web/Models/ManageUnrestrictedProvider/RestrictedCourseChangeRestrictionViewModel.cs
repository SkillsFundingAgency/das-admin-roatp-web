using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class RestrictedCourseChangeRestrictionViewModel : RestrictedCourseChangeRestrictionSubmitModel, IBackLink
{
    public int Ukprn { get; set; }
    public required string LarsCode { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public DateTime? LastDateStarts { get; set; }
    public required string CancelUrl { get; set; }

    public bool HasLastDateStarts => LastDateStarts.HasValue;
    public string? LastDateStartsText => LastDateStarts?.ToDisplayString();
}
