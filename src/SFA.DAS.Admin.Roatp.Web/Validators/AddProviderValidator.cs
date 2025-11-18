using FluentValidation;
using SFA.DAS.Admin.Roatp.Web.Models;
using SFA.DAS.Admin.Roatp.Web.Validators.Common;

namespace SFA.DAS.Admin.Roatp.Web.Validators;

public class AddProviderValidator : AbstractValidator<AddProviderSubmitModel>
{
    public const string UkprnFormatValidationMessage = "Enter a UKPRN using 8 digits";

    public AddProviderValidator()
    {
        RuleFor(s => s.Ukprn!)
            .MustBeValidUkprnFormat();
    }
}