using Humanizer;
using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Extensions;

namespace SFA.DAS.Admin.Roatp.Web.Models.ManageCourses;

public class UnrestrictedCourseDetailsViewModel : ICourseDisplayModel, IBackLink, ILarsCode
{
    public required string LarsCode { get; set; }
    public required string CourseName { get; set; }
    public int Level { get; set; }
    public required string Title { get; set; }
    public required string Sector { get; set; }
    public LearningType LearningType { get; set; }
    public IEnumerable<UnrestrictedCourseProviderViewModel> Providers { get; set; } = [];
    public string DisplayTitle => this.GetDisplayTitle();
    public string LearningTypeDescription => LearningType.GetDescription();
    public string StatusText { get; init; } = "Unrestricted";
    public bool HasProviders => Providers.Any();
    public bool HasNoProviders => !HasProviders;
    public int ProviderCount => Providers.Count();
    public string ProviderCountDescription => "provider".ToQuantity(ProviderCount);

    public static implicit operator UnrestrictedCourseDetailsViewModel(
        GetRestrictedCourseDetailsResponse response)
    {
        var providers = response.Providers
            .OrderBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .Select(provider => (UnrestrictedCourseProviderViewModel)provider)
            .ToList();

        return new()
        {
            LarsCode = response.LarsCode,
            CourseName = response.CourseName,
            Level = response.Level,
            Title = response.CourseName,
            Sector = response.Route,
            LearningType = response.LearningType,
            Providers = providers,
            StatusText = "Unrestricted"
        };
    }
}
