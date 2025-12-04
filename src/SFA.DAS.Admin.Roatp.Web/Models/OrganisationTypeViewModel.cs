using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class OrganisationTypeViewModel : OrganisationTypeSubmitModel, IBackLink
{
    public required string LegalName { get; set; }
    public List<OrganisationTypeModel> OrganisationTypes { get; set; } = [];
}
public class OrganisationTypeSubmitModel
{
    public int? OrganisationTypeId { get; set; }
}