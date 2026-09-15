namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class RestrictedApprenticeshipModel
{
    public required string LarsCode { get; set; }
    public required string Title { get; set; }
    public int Level { get; set; }
    public DateTime? LastDateStarts { get; set; }
    public bool IsClosedToNewStarts { get; set; }
}
