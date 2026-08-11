namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class AddLastDateStartsSubmitModel
{
    public string? Day { get; set; }
    public string? Month { get; set; }
    public string? Year { get; set; }
    public string? LarsCode { get; set; }
    public DateTime? CourseLastDateStarts { get; set; }

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
            date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
