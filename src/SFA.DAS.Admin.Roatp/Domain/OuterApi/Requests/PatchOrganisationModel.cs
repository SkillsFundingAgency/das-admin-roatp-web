using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
public class PatchOrganisationModel
{
    public OrganisationStatus Status { get; set; }
    public int? RemovedReasonId { get; set; }
    public ProviderType ProviderType { get; set; }
    public int OrganisationTypeId { get; set; }

    public static implicit operator PatchOrganisationModel(GetOrganisationResponse organisationResponse)
    {
        return new PatchOrganisationModel
        {
            Status = organisationResponse.Status,
            OrganisationTypeId = organisationResponse.OrganisationTypeId,
            ProviderType = organisationResponse.ProviderType,
            RemovedReasonId = organisationResponse.RemovedReasonId
        };
    }
}
