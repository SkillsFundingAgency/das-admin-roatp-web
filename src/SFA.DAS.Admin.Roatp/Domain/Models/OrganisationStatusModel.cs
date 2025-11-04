namespace SFA.DAS.Admin.Roatp.Domain.Models;
public class OrganisationStatusModel
{
    public required int Id { get; set; }
    public required string Description { get; set; }
    public bool IsSelected { get; set; }
}
