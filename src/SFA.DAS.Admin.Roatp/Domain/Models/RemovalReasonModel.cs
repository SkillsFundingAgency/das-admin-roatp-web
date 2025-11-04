namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class RemovalReasonModel : RadioButtonSelectionModel
{
    public required int Id { get; set; }
    public required string Description { get; set; }
}
