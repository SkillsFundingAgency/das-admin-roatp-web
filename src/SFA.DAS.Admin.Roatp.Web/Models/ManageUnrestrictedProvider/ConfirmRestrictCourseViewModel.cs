namespace SFA.DAS.Admin.Roatp.Web.Models.ManageUnrestrictedProvider;

public class ConfirmRestrictCourseViewModel
{
    public required int Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public required string DisplayTitle { get; set; }
    public required string LarsCode { get; set; }
    public required string CancelUrl { get; set; }
}
