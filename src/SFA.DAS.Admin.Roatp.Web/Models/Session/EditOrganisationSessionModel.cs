using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Domain.Models;

public class EditOrganisationSessionModel
{
    public required Guid OrganisationId { get; set; }
    public required int Ukprn { get; set; }
    public required string LegalName { get; set; }
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

    public static implicit operator EditOrganisationSessionModel(GetOrganisationResponse organisationResponse)
    {
        var organisationSessionModel = new EditOrganisationSessionModel
        {
            OrganisationId = organisationResponse.OrganisationId,
            Ukprn = organisationResponse.Ukprn,
            LegalName = organisationResponse.LegalName,
            TradingName = organisationResponse.TradingName,
            CompanyNumber = organisationResponse.CompanyNumber,
            CharityNumber = organisationResponse.CharityNumber,
            ProviderType = organisationResponse.ProviderType,
            OrganisationTypeId = organisationResponse.OrganisationTypeId,
            OrganisationType = organisationResponse.OrganisationType,
            Status = organisationResponse.Status,
            LastUpdatedDate = organisationResponse.LastUpdatedDate,
            ApplicationDeterminedDate = organisationResponse.ApplicationDeterminedDate,
            RemovedReasonId = organisationResponse.RemovedReasonId,
            RemovedReason = organisationResponse.RemovedReason,
            RemovedDate = organisationResponse.RemovedDate,
            AllowedCourseTypes = organisationResponse.AllowedCourseTypes
        };

        return organisationSessionModel;
    }
}