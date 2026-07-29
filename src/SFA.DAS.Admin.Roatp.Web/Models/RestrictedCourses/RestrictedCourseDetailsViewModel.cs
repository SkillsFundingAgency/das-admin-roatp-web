using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.RestrictedCourses;

public class RestrictedCourseDetailsViewModel : ICourseDisplayModel, IBackLink
{
    public required string LarsCode { get; set; }
    public required string CourseName { get; set; }
    public int Level { get; set; }
    public required string Title { get; set; }
    public required string Sector { get; set; }
    public LearningType LearningType { get; set; }
    public bool IsCourseRestricted { get; set; }
    public IEnumerable<AllowedProviderViewModel> AllowedProviders { get; set; } = [];

    public string DisplayTitle => this.GetDisplayTitle();
    public string LearningTypeDescription => LearningType.GetDescription();
    public string StatusText => IsCourseRestricted ? "Restricted" : "Unrestricted";
    public bool HasProviders => AllowedProviders.Any();
    public bool HasNoProviders => !HasProviders;
    public int ProviderCount => AllowedProviders.Count();
    public string ProviderCountDescription => "provider".ToQuantity(ProviderCount);

    public string RestrictedCourseDetailsPageUrl { get; set; } = "#";


    public static implicit operator RestrictedCourseDetailsViewModel(
        GetRestrictedCourseDetailsResponse response) => new()
        {
            LarsCode = response.LarsCode,
            CourseName = response.CourseName,
            Level = response.Level,
            Title = response.CourseName,
            Sector = response.Route,
            LearningType = response.LearningType,
            IsCourseRestricted = response.IsCourseRestricted,
            AllowedProviders = response.Providers
            .OrderBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .Select(provider => (AllowedProviderViewModel)provider)
            .ToList()
        };
}
