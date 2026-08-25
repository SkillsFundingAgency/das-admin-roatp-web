using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public class UkprnAndLarsCodeValidator : AbstractValidator<IUkprnAndLarsCodeValidator>
{
    public UkprnAndLarsCodeValidator(
        IValidator<IUkprn> ukprnValidator,
        IValidator<ILarsCode> larsCodeValidator)
    {
        Include(ukprnValidator);
        Include(larsCodeValidator);
    }
}
