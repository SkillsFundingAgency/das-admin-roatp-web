using SFA.DAS.Admin.Roatp.Domain.Models;
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

    public static implicit operator ProviderSummaryViewModel(EditOrganisationSessionModel savedOrganisation)
    {
        var trainingProviderViewModel = new ProviderSummaryViewModel
        {
            Ukprn = savedOrganisation.Ukprn,
            LegalName = savedOrganisation.LegalName,
            TradingName = savedOrganisation.TradingName,
            ShowTradingName = !string.IsNullOrWhiteSpace(savedOrganisation.TradingName),
            CompanyNumber = !string.IsNullOrWhiteSpace(savedOrganisation.CompanyNumber) ? savedOrganisation.CompanyNumber : "Not applicable",
            ShowCompanyNumber = !string.IsNullOrWhiteSpace(savedOrganisation.CompanyNumber),
            ProviderType = savedOrganisation.ProviderType,
            IsSupportingProvider = savedOrganisation.ProviderType == ProviderType.Supporting,
            OrganisationType = savedOrganisation.OrganisationType,
            Status = savedOrganisation.Status,
            LastUpdatedDateText = savedOrganisation.LastUpdatedDate.HasValue ? savedOrganisation.LastUpdatedDate.Value.ToScreenString() : string.Empty,
            ShowLastUpdatedDate = savedOrganisation.LastUpdatedDate.HasValue,
            ApplicationDeterminedDateText = savedOrganisation.ApplicationDeterminedDate.HasValue ? savedOrganisation.ApplicationDeterminedDate.Value.ToScreenString() : "Unavailable",
            ShowRemovedReason = savedOrganisation.RemovedReasonId.HasValue,
            RemovedReason = savedOrganisation.RemovedReason,
            RemovedDateText = savedOrganisation.RemovedDate.HasValue ? savedOrganisation.RemovedDate.Value.ToScreenString() : string.Empty,
            AllowedCourseTypes = savedOrganisation.AllowedCourseTypes,
            OffersApprenticeshipsText = savedOrganisation.AllowedCourseTypes.Any(x => x.LearningType == LearningType.Standard) ? "Yes" : "No",
            OffersShortCoursesText = savedOrganisation.AllowedCourseTypes.Any(x => x.LearningType == LearningType.ShortCourse) ? "Yes" : "No",
            TypesOfShortCourses = savedOrganisation.AllowedCourseTypes.Where(x => x.LearningType == LearningType.ShortCourse).Select(x => x.CourseTypeName).OrderBy(x => x).ToList(),
            CharityNumberText = !string.IsNullOrWhiteSpace(savedOrganisation.CharityNumber) ? savedOrganisation.CharityNumber : "Not applicable",
            IsActive = savedOrganisation.Status == OrganisationStatus.Active,
            IsActiveNoStarts = savedOrganisation.Status == OrganisationStatus.ActiveNoStarts,
            IsOnboarding = savedOrganisation.Status == OrganisationStatus.OnBoarding,
            IsRemoved = savedOrganisation.Status == OrganisationStatus.Removed
        };

        return trainingProviderViewModel;
    }
}
