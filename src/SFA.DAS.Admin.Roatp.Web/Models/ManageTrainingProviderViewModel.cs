namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ManageTrainingProviderViewModel : ICustomBackLink
{
    public string SearchForTrainingProviderUrl { get; set; } = "#";
    public string AddUkprnToAllowListUrl { get; set; } = "#";
    public string AddANewTrainingProviderUrl { get; set; } = "#";
    public string ViewRestrictedCoursesUrl { get; set; } = "#";

    public string BackLinkUrl { get; set; } = "#";
    public required string BackLinkText { get; set; }
}
