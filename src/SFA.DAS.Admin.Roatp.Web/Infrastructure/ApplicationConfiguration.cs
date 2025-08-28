namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public class ApplicationConfiguration
{
    public required string RedisConnectionString { get; set; }
    public required string DataProtectionKeysDatabase { get; set; }
    public required string DfESignInServiceHelpUrl { get; set; }
    public required string AdminServicesBaseUrl { get; set; }
}
