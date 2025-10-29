using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
public class PatchOrganisationModel
{
    public OrganisationStatus Status { get; set; }
    public int? RemovedReasonId { get; set; }
    public ProviderType ProviderType { get; set; }
    public int OrganisationTypeId { get; set; }
}
