using FluentValidation;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public static class UkprnValidator
{
    public const string UkprnFormatValidationMessage = "Enter a UKPRN using 8 digits";

    public static IRuleBuilderOptions<T, string> MustBeValidUkprnFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .NotEmpty()
            .WithMessage(UkprnFormatValidationMessage)
            .Matches(@"^1\d{7}$")
            .WithMessage(UkprnFormatValidationMessage);
}
