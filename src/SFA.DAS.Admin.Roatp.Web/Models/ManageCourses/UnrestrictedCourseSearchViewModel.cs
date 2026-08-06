namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class UnrestrictedCourseSearchViewModel : IBackLink
{
    public string? SearchTerm =>
        string.IsNullOrWhiteSpace(Title)
            ? string.Empty
            : Level.HasValue
                ? $"{Title} (Level {Level})"
                : Title;

    public string? LarsCode { get; set; }
    public string? Title { get; set; }
    public int? Level { get; set; }
}
