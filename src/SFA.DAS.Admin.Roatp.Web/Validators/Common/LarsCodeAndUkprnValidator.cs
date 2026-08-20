using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public class LarsCodeAndUkprnValidator(
    LarsCodeValidator larsCodeValidator,
    UkprnValidator ukprnValidator)
{
    public async Task<bool> IsValidAsync(
        string larsCode,
        int ukprn,
        CancellationToken cancellationToken)
    {
        var larsCodeValidationResult = await larsCodeValidator.ValidateAsync(
            new LarsCodeModel { LarsCode = larsCode },
            cancellationToken);

        if (!larsCodeValidationResult.IsValid)
        {
            return false;
        }

        var ukprnValidationResult = await ukprnValidator.ValidateAsync(
            new UkprnModel { Ukprn = ukprn },
            cancellationToken);

        return ukprnValidationResult.IsValid;
    }
}
