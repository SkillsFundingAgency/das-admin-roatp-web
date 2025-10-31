using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public class ProviderSummaryViewModel : ISearchProviderLink
{
    public int Ukprn { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string TradingName { get; set; } = string.Empty;
    public string CompanyNumber { get; set; } = string.Empty;
    public ProviderType ProviderType { get; set; }

    public string OrganisationType { get; set; } = string.Empty;
    public OrganisationStatus Status { get; set; }
    public bool ShowRemovedReason { get; set; }
    public string RemovedReason { get; set; } = string.Empty;
    public string RemovedDateText { get; set; } = string.Empty;
    public IEnumerable<AllowedCourseType> AllowedCourseTypes { get; set; } = [];

    public List<string> TypesOfShortCourses { get; set; } = new();

    public bool ShowTradingName { get; set; }
    public bool ShowCompanyNumber { get; set; }
    public bool ShowLastUpdatedDate { get; set; }
    public string LastUpdatedDateText { get; set; } = string.Empty;
    public string OffersApprenticeshipsText { get; set; } = string.Empty;
    public string OffersShortCoursesText { get; set; } = string.Empty;
    public string CharityNumberText { get; set; } = string.Empty;
    public string ApplicationDeterminedDateText { get; set; } = string.Empty;

    public bool IsSupportingProvider { get; set; }

    public string SearchProviderUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public bool IsActiveNoStarts { get; set; }
    public bool IsOnboarding { get; set; }
    public bool IsRemoved { get; set; }
    public string StatusChangeLink { get; set; } = "#";
    public string ProviderTypeChangeLink { get; set; } = "#";
    public string OrganisationTypeChangeLink { get; set; } = "#";
    public string OffersShortCoursesChangeLink { get; set; } = "#";
    public string TypesOfShortCoursesChangeLink { get; set; } = "#";

    public static implicit operator ProviderSummaryViewModel(GetOrganisationResponse organisationResponse)
    {
        var trainingProviderViewModel = new ProviderSummaryViewModel
        {
            Ukprn = organisationResponse.Ukprn,
            LegalName = organisationResponse.LegalName,
            TradingName = organisationResponse.TradingName,
            ShowTradingName = !string.IsNullOrWhiteSpace(organisationResponse.TradingName),
            CompanyNumber = !string.IsNullOrWhiteSpace(organisationResponse.CompanyNumber) ? organisationResponse.CompanyNumber : "Not applicable",
            ShowCompanyNumber = !string.IsNullOrWhiteSpace(organisationResponse.CompanyNumber),
            ProviderType = organisationResponse.ProviderType,
            IsSupportingProvider = organisationResponse.ProviderType == ProviderType.Supporting,
            OrganisationType = organisationResponse.OrganisationType,
            Status = organisationResponse.Status,
            LastUpdatedDateText = organisationResponse.LastUpdatedDate.HasValue ? organisationResponse.LastUpdatedDate.Value.ToScreenString() : string.Empty,
            ShowLastUpdatedDate = organisationResponse.LastUpdatedDate.HasValue,
            ApplicationDeterminedDateText = organisationResponse.ApplicationDeterminedDate.HasValue ? organisationResponse.ApplicationDeterminedDate.Value.ToScreenString() : "Unavailable",
            ShowRemovedReason = organisationResponse.RemovedReasonId.HasValue,
            RemovedReason = organisationResponse.RemovedReason,
            RemovedDateText = organisationResponse.RemovedDate.HasValue ? organisationResponse.RemovedDate.Value.ToScreenString() : string.Empty,
            AllowedCourseTypes = organisationResponse.AllowedCourseTypes,
            OffersApprenticeshipsText = organisationResponse.AllowedCourseTypes.Any(x => x.LearningType == LearningType.Standard) ? "Yes" : "No",
            OffersShortCoursesText = organisationResponse.AllowedCourseTypes.Any(x => x.LearningType == LearningType.ShortCourse) ? "Yes" : "No",
            TypesOfShortCourses = organisationResponse.AllowedCourseTypes.Where(x => x.LearningType == LearningType.ShortCourse).Select(x => x.CourseTypeName).OrderBy(x => x).ToList(),
            CharityNumberText = !string.IsNullOrWhiteSpace(organisationResponse.CharityNumber) ? organisationResponse.CharityNumber : "Not applicable",
            IsActive = organisationResponse.Status == OrganisationStatus.Active,
            IsActiveNoStarts = organisationResponse.Status == OrganisationStatus.ActiveNoStarts,
            IsOnboarding = organisationResponse.Status == OrganisationStatus.OnBoarding,
            IsRemoved = organisationResponse.Status == OrganisationStatus.Removed
        };

        return trainingProviderViewModel;
    }
}
