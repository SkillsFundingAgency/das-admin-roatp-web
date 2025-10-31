using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class RouteNames
{
    public const string Home = "Home";
    public const string Dashboard = "dashboard";
    public const string SelectProvider = nameof(SelectProvider);
    public const string ProviderSummary = nameof(ProviderSummary);
}
