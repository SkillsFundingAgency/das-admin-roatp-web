using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

[ExcludeFromCodeCoverage]
public class GetUkrlpResponse
{
    public string? LegalName { get; set; }
    public string? TradingName { get; set; }
    public string? CharityNumber { get; set; }
    public string? CompanyNumber { get; set; }
}