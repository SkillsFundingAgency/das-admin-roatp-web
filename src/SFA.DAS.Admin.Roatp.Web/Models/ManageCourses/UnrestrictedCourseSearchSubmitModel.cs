namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class UnrestrictedCourseSearchSubmitModel
{
    public string? SearchTerm =>
        string.IsNullOrWhiteSpace(Title)
            ? string.Empty
            : $"{Title} (Level {Level})";

    public string? Title { get; set; }
    public int? Level { get; set; }
}
