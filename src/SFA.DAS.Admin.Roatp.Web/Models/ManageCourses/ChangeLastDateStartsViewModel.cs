using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class ChangeLastDateStartsViewModel : ChangeLastDateStartsSubmitModel, IBackLink
{
    public string LarsCode { get; set; } = null!;
    public int Ukprn { get; set; }
    public string ProviderName { get; set; } = null!;
    public string CourseDisplayTitle { get; set; } = null!;
    public DateTime LastDateStarts { get; set; }
    public string CancelUrl { get; set; } = "#";

    public string LastDateStartsText => LastDateStarts.ToScreenString();
}
