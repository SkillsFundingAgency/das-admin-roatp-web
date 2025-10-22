using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class RouteNames
{
    public const string Dashboard = "dashboard";
    public const string RegisteredProviders = nameof(RegisteredProviders);
    public const string SelectProvider = nameof(SelectProvider);
}
