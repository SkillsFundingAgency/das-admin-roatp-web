using SFA.DAS.Admin.Roatp.Domain.Models;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
public class GetOrganisationResponse
{
    public Guid OrganisationId { get; set; }
    public int Ukprn { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string TradingName { get; set; } = string.Empty;
    public string CompanyNumber { get; set; } = string.Empty;
    public string CharityNumber { get; set; } = string.Empty;
    public ProviderType ProviderType { get; set; }
    public int OrganisationTypeId { get; set; }
    public string OrganisationType { get; set; } = string.Empty;
    public OrganisationStatus Status { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public DateTime? ApplicationDeterminedDate { get; set; }
    public int? RemovedReasonId { get; set; }
    public string RemovedReason { get; set; } = string.Empty;
    public DateTime? RemovedDate { get; set; }
    public IEnumerable<AllowedCourseType> AllowedCourseTypes { get; set; } = [];
}
