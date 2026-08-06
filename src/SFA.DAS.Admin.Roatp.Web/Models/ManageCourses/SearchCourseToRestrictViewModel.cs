namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class SearchCourseToRestrictViewModel : IBackLink
{
    public string? SearchTerm =>
        string.IsNullOrWhiteSpace(LarsCode)
            ? string.Empty
            : $"{Title} (Level {Level})";

    public string? LarsCode { get; set; }
    public string? Title { get; set; }
    public int Level { get; set; }
}
