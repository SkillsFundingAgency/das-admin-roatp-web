using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class RouteNames
{
    public const string Home = "Home";
    public const string Dashboard = "dashboard";
    public const string SelectProvider = nameof(SelectProvider);
    public const string ProviderSummary = nameof(ProviderSummary);
    public const string ProviderStatusUpdate = nameof(ProviderStatusUpdate);
    public const string ProviderRemovalReasonUpdate = nameof(ProviderRemovalReasonUpdate);
    public const string ProviderStatusUpdateConfirmed = nameof(ProviderStatusUpdateConfirmed);
    public const string ProviderTypeUpdate = nameof(ProviderTypeUpdate);
    public const string OrganisationTypeUpdate = nameof(OrganisationTypeUpdate);
    public const string ApprenticeshipUnitsUpdate = nameof(ApprenticeshipUnitsUpdate);
}
