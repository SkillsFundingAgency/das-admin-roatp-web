using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Requests;
public class PostOrganisationCommand
{
    public int Ukprn { get; set; }
    public required string LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? CompanyNumber { get; set; }
    public string? CharityNumber { get; set; }
    public ProviderType ProviderType { get; set; }
    public int OrganisationTypeId { get; set; }
    public required string RequestingUserId { get; set; }
    public required string RequestingUserDisplayName { get; set; }
    public bool DeliversApprenticeships { get; set; }
    public bool DeliversApprenticeshipUnits { get; set; }
}