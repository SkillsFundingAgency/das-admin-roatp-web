using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;
using SFA.DAS.Admin.Roatp.Web.Models.Filters;
using SFA.DAS.Admin.Roatp.Web.Models.Shared;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class RestrictedCourseDetailsViewModel : ICourseDisplayModel, IBackLink, ILarsCode
{
    public required string LarsCode { get; set; }
    public required string CourseName { get; set; }
    public int Level { get; set; }
    public required string Title { get; set; }
    public required string Sector { get; set; }
    public LearningType LearningType { get; set; }
    public bool IsCourseRestricted { get; set; }
    public IEnumerable<AllowedProviderViewModel> AllowedProviders { get; set; } = [];
    public int TotalProviderCount { get; set; }

    public string DisplayTitle => this.GetDisplayTitle();
    public string LearningTypeDescription => LearningType.GetDescription();
    public string StatusText => IsCourseRestricted ? "Restricted" : "Unrestricted";
    public bool HasProviders => TotalProviderCount > 0;
    public bool HasNoProviders => !HasActiveFilters && !HasProviders;
    public bool HasNoFilterResults => HasActiveFilters && !HasProviders;
    public int ProviderCount => TotalProviderCount;
    public string ProviderCountDescription => "provider".ToQuantity(ProviderCount);

    public string RestrictedCourseDetailsPageUrl { get; set; } = "#";
    public bool HasActiveFilters { get; set; }
    public FiltersViewModel Filters { get; set; } = new() { Route = string.Empty };
    public PaginationViewModel Pagination { get; set; } = null!;

    public static implicit operator RestrictedCourseDetailsViewModel(
        GetRestrictedCourseDetailsResponse response)
    {
        var providers = response.Providers
            .OrderBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .Select(provider => (AllowedProviderViewModel)provider)
            .ToList();

        return new()
        {
            LarsCode = response.LarsCode,
            CourseName = response.CourseName,
            Level = response.Level,
            Title = response.CourseName,
            Sector = response.Route,
            LearningType = response.LearningType,
            IsCourseRestricted = response.IsCourseRestricted,
            AllowedProviders = providers,
            TotalProviderCount = providers.Count
        };
    }
}
