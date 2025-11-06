using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class OrganisationStatusUpdateViewModel : IBackLink
{
    public List<OrganisationStatusSelectionModel> OrganisationStatuses { get; set; } = new();
    public required OrganisationStatus OrganisationStatus { get; set; }

}