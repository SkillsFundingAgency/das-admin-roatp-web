using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class AddLastDateStartsViewModel : IBackLink
{
    public required string LarsCode { get; set; }
    public required int Ukprn { get; set; }
    public required string ProviderName { get; set; }
    public required string CourseDisplayTitle { get; set; }
    public string Day { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public DateTime? CourseLastDateStarts { get; set; }
    public string CancelUrl { get; set; } = "#";

    public bool TryGetEnteredDate(out DateTime date)
    {
        date = default;

        if (!int.TryParse(Day, out var day)
            || !int.TryParse(Month, out var month)
            || !int.TryParse(Year, out var year))
        {
            return false;
        }

        try
        {
            date = new DateTime(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public string CourseLastDateStartsText =>
        CourseLastDateStarts.HasValue ? CourseLastDateStarts.Value.ToScreenString() : string.Empty;
}
