using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public class OuterApiHealthCheck(IOuterApiClient _outerApiClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var response = await _outerApiClient.Ping(cancellationToken);

        return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
