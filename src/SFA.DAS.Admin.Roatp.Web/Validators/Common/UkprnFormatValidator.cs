using FluentValidation;

namespace SFA.DAS.Admin.Roatp.Web.Validators.Common;

public static class UkprnFormatValidator
{
    public const string UkprnFormatValidationMessage = "Enter a UKPRN using 8 digits";
    private const string UkprnFormatRegex = @"^1\d{7}$";

    public static IRuleBuilderOptions<T, string> MustBeValidUkprnFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .NotEmpty()
            .WithMessage(UkprnFormatValidationMessage)
            .Matches(UkprnFormatRegex)
            .WithMessage(UkprnFormatValidationMessage);
}
