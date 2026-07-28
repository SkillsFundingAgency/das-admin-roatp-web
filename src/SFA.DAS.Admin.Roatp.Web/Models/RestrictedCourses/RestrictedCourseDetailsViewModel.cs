using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class AllowedProviderItemViewModel
{
    public int Ukprn { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime? DateLastStarts { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }

    public string DeliveryStatusDescription => DeliveryStatus.GetDescription();
    public string DeliveryStatusTagClass => DeliveryStatus.GetTagClass();
    public bool HasLastStartDate => DateLastStarts.HasValue;
    public string LastStartDateText => DateLastStarts.HasValue ? DateLastStarts.Value.ToScreenString() : string.Empty;
    public string ChangeUrl { get; set; } = "#";

    public static implicit operator AllowedProviderItemViewModel(ProviderCourseModel provider) => new()
    {
        Ukprn = provider.Ukprn,
        ProviderName = provider.ProviderName,
        DateLastStarts = provider.DateLastStarts,
        DeliveryStatus = provider.DateLastStarts.ToDeliveryStatus()
    };
}

public class RestrictedCourseDetailsViewModel : ICustomBackLink
{
    public required string LarsCode { get; set; }
    public required string CourseName { get; set; }
    public int Level { get; set; }
    public required string Sector { get; set; }
    public LearningType LearningType { get; set; }
    public bool IsCourseRestricted { get; set; }
    public IEnumerable<AllowedProviderItemViewModel> Providers { get; set; } = [];

    public string DisplayTitle => Level > 0 ? $"{CourseName} (Level {Level})" : CourseName;
    public string LearningTypeDescription => LearningType.GetDescription();
    public string StatusText => IsCourseRestricted ? "Restricted" : "Not restricted";
    public bool HasProviders => Providers.Any();
    public bool HasNoProviders => !HasProviders;
    public int ProviderCount => Providers.Count();
    public string ProviderCountDescription => "provider".ToQuantity(ProviderCount);

    public string BackLinkUrl { get; set; } = "#";
    public string BackLinkText => "Back to restricted courses";
    public string PageUrl { get; set; } = "#";

    public static implicit operator RestrictedCourseDetailsViewModel(
        GetRestrictedCourseDetailsResponse response) => new()
        {
            LarsCode = response.LarsCode,
            CourseName = response.CourseName,
            Level = response.Level,
            Sector = response.Route,
            LearningType = response.LearningType,
            IsCourseRestricted = response.IsCourseRestricted,
            Providers = response.Providers
            .OrderBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .Select(provider => (AllowedProviderItemViewModel)provider)
            .ToList()
        };
}
