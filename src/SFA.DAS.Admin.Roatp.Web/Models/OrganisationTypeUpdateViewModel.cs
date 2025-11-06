using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class OrganisationTypeUpdateViewModel : IBackLink
{
    public required string LegalName { get; set; }
    public List<OrganisationTypeModel> OrganisationTypes { get; set; } = [];
    public required int OrganisationTypeId { get; set; }
}