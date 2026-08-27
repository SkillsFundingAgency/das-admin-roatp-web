using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class SetLastDateStartsViewModel : IBackLink
{
    public string LarsCode { get; set; } = null!;
    public int Ukprn { get; set; }
    public string ProviderName { get; set; } = null!;
    public string CourseDisplayTitle { get; set; } = null!;
    public string? Day { get; set; }
    public string? Month { get; set; }
    public string? Year { get; set; }
    public DateTime? CourseLastDateStarts { get; set; }
    public bool IsChangingExistingDate { get; set; }
    public bool IsAddingLastDateStarts => !IsChangingExistingDate;
    public string CancelUrl { get; set; } = "#";

    public string CourseLastDateStartsText =>
        CourseLastDateStarts.HasValue ? CourseLastDateStarts.Value.ToDisplayString() : string.Empty;
}
