namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class OrganisationTypeModel : RadioButtonSelectionModel
{
    public required int Id { get; set; }
    public required string Description { get; set; }
}