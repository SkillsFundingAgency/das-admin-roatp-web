using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Models;

public sealed class UkprnAndLarsCodeModel : IUkprnAndLarsCodeValidator
{
    public required int Ukprn { get; init; }
    public required string LarsCode { get; init; }
}
