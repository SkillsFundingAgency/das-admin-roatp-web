using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;


public class OrganisationStatusUpdateViewModel : IBackLink
{
    public List<OrganisationStatusModel> OrganisationStatuses { get; set; }
    public required OrganisationStatus OrganisationStatus { get; set; }

}