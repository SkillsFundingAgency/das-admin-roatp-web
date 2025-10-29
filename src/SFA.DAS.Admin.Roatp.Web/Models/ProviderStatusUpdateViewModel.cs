using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Web.Models;


public class OrganisationStatusUpdateViewModel : IBackLink
{
    public required OrganisationStatus OrganisationStatus { get; set; }
    public required string Ukprn { get; set; }
}