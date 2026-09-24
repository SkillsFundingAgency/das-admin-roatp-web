using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class ChangeProviderRestrictedCourseViewModel : ChangeProviderRestrictedCourseSubmitModel, IBackLink
{
    public int Ukprn { get; set; }
    public required string LarsCode { get; set; }
    public required string DisplayTitle { get; set; }
    public DateTime? LastDateStarts { get; set; }
    public required string CancelUrl { get; set; }

    public bool HasLastDateStarts => LastDateStarts.HasValue;
    public string? LastDateStartsText => LastDateStarts?.ToDisplayString();
}
